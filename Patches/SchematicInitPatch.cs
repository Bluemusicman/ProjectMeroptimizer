using AdminToys;
using HarmonyLib;
using Mirror;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using UnityEngine;

namespace MerOptimizer.Patches;

/// <summary>
/// Patches <see cref="SchematicObject.Init"/> to:
/// 1. Register every spawned NetworkIdentity with <see cref="ProximityCullingManager"/>.
/// 2. Strip Collidable flag from tiny primitives.
/// 3. Force NetworkIsStatic on non-animated primitives.
///
/// We patch AFTER the original method finishes so all blocks are already created.
/// </summary>
[HarmonyPatch(typeof(SchematicObject), nameof(SchematicObject.Init))]
internal static class SchematicInitPatch
{
    [HarmonyPostfix]
    public static void Postfix(SchematicObject __instance, SchematicObjectDataList data)
    {
        try
        {
            ApplyOptimizations(__instance);
        }
        catch (Exception ex)
        {
            LabApi.Features.Console.Logger.Error($"[MerOptimizer] SchematicInitPatch error: {ex}");
        }
    }

    private static void ApplyOptimizations(SchematicObject schematic)
    {
        Config cfg = MerOptimizerPlugin.Instance.Config;

        // Attach culling tracker to the schematic root
        SchematicCullingTracker tracker = schematic.gameObject.AddComponent<SchematicCullingTracker>();

        int stripped = 0, staticForced = 0, registered = 0;

        foreach (GameObject block in schematic.AttachedBlocks)
        {
            if (block == null) continue;

            // Register networked objects with culling manager
            if (cfg.EnableProximityCulling && block.TryGetComponent(out NetworkIdentity netId))
            {
                tracker.TrackedIdentities.Add(netId);
                registered++;
            }

            if (!block.TryGetComponent(out PrimitiveObjectToy prim)) continue;

            // ── Collision strip for tiny objects ──────────────────────────────
            if (cfg.AutoStripTinyColliders)
            {
                Vector3 s = block.transform.lossyScale;
                float maxAxis = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));

                if (maxAxis < cfg.TinyColliderMaxScale)
                {
                    // Remove Collidable bit but keep Visible
                    prim.NetworkPrimitiveFlags = prim.NetworkPrimitiveFlags & ~PrimitiveFlags.Collidable;
                    stripped++;
                }
            }

            // ── Force static on non-animated primitives ───────────────────────
            if (cfg.ForceStaticOnNonAnimated)
            {
                // If there's no Animator component the primitive is static
                if (!block.TryGetComponent<Animator>(out _))
                {
                    prim.NetworkIsStatic = true;
                    staticForced++;
                }
            }
        }

        // Register tracker after we're done building the list
        if (cfg.EnableProximityCulling)
            ProximityCullingManager.Register(tracker);

        if (cfg.DebugStats)
        {
            LabApi.Features.Console.Logger.Debug(
                $"[MerOptimizer] Schematic '{schematic.Name}' — " +
                $"registered={registered}, collisionStripped={stripped}, staticForced={staticForced}");
        }
    }
}
