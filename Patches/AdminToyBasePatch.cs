using AdminToys;
using HarmonyLib;
using Mirror;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace MerOptimizer.Patches;

/// <summary>
/// Patches <see cref="AdminToyBase.OnStartServer"/> to register standalone primitive/light/speaker toys
/// with the proximity culling manager.
/// </summary>
[HarmonyPatch(typeof(AdminToyBase), nameof(AdminToyBase.OnStartServer))]
internal static class AdminToyBasePatch
{
    [HarmonyPostfix]
    public static void Postfix(AdminToyBase __instance)
    {
        try
        {
            Config cfg = MerOptimizerPlugin.Instance.Config;
            if (!cfg.EnableStandaloneToyCulling) return;

            // If the toy is child of a schematic, the SchematicCullingTracker handles it.
            if (__instance.GetComponentInParent<SchematicObject>() != null)
                return;

            if (__instance.TryGetComponent(out NetworkIdentity netId))
            {
                // Register with culling manager
                ProximityCullingManager.RegisterStandaloneToy(netId);

                // Add a small script to automatically unregister this standalone toy when destroyed
                __instance.gameObject.AddComponent<StandaloneToyTracker>();
            }
        }
        catch (Exception ex)
        {
            LabApi.Features.Console.Logger.Error($"[MerOptimizer] AdminToyBasePatch error: {ex}");
        }
    }
}

/// <summary>
/// Automatically unregisters standalone toys from proximity culling when they are destroyed.
/// </summary>
internal sealed class StandaloneToyTracker : MonoBehaviour
{
    private NetworkIdentity? _netId;

    private void Awake()
    {
        _netId = GetComponent<NetworkIdentity>();
    }

    private void OnDestroy()
    {
        if (_netId != null)
        {
            ProximityCullingManager.UnregisterStandaloneToy(_netId);
        }
    }
}
