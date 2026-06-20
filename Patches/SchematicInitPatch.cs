using AdminToys;
using HarmonyLib;
using Mirror;
using ProjectMER.Features.Objects;
using ProjectMER.Features.Serializable.Schematics;
using UnityEngine;

namespace MerOptimizer.Patches;

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

        SchematicCullingTracker tracker = schematic.gameObject.AddComponent<SchematicCullingTracker>();

        int stripped = 0, staticForced = 0, registered = 0;

        foreach (GameObject block in schematic.AttachedBlocks)
        {
            if (block == null) continue;

            if (cfg.EnableProximityCulling && block.TryGetComponent(out NetworkIdentity netId))
            {
                tracker.TrackedIdentities.Add(netId);
                registered++;
            }

            if (!block.TryGetComponent(out PrimitiveObjectToy prim)) continue;

            if (cfg.AutoStripTinyColliders)
            {
                Vector3 s = block.transform.lossyScale;
                float maxAxis = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));

                if (maxAxis < cfg.TinyColliderMaxScale)
                {
                    prim.NetworkPrimitiveFlags = prim.NetworkPrimitiveFlags & ~PrimitiveFlags.Collidable;
                    stripped++;
                }
            }

            if (cfg.ForceStaticOnNonAnimated)
            {
                if (!block.TryGetComponent<Animator>(out _))
                {
                    prim.NetworkIsStatic = true;
                    staticForced++;
                }
            }
        }

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
