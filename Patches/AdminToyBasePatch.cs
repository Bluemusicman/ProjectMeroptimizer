using AdminToys;
using HarmonyLib;
using Mirror;
using ProjectMER.Features.Objects;
using UnityEngine;

namespace MerOptimizer.Patches;

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

            if (__instance.GetComponentInParent<SchematicObject>() != null)
                return;

            if (__instance.TryGetComponent(out NetworkIdentity netId))
            {
                ProximityCullingManager.RegisterStandaloneToy(netId);
                __instance.gameObject.AddComponent<StandaloneToyTracker>();
            }
        }
        catch (Exception ex)
        {
            LabApi.Features.Console.Logger.Error($"[MerOptimizer] AdminToyBasePatch error: {ex}");
        }
    }
}

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
