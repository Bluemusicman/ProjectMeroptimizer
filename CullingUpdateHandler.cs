using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using UnityEngine;

namespace MerOptimizer;

/// <summary>
/// Drives the periodic proximity-culling update loop and handles item pickup lifecycle events.
/// Uses a plain Unity Coroutine (WaitForSeconds) so there is no MEC dependency.
/// </summary>
internal sealed class CullingUpdateHandler : CustomEventsHandler
{
    private CullingRunner? _runner;
    private readonly Config _cfg;

    public CullingUpdateHandler(Config cfg) => _cfg = cfg;

    public override void OnServerRoundStarted()
    {
        if (_cfg.EnableProximityCulling || _cfg.EnableStandaloneToyCulling || _cfg.EnablePickupCulling)
            StartLoop();
    }

    public override void OnServerRoundEnded(RoundEndedEventArgs args)
    {
        StopLoop();
        ProximityCullingManager.Clear();
    }

    public override void OnServerWaitingForPlayers()
    {
        StopLoop();
        ProximityCullingManager.Clear();
    }

    // ── Item Pickup Events ───────────────────────────────────────────────────

    public override void OnServerPickupCreated(PickupCreatedEventArgs ev)
    {
        if (_cfg.EnablePickupCulling && ev?.Pickup?.NetworkIdentity != null)
            ProximityCullingManager.RegisterPickup(ev.Pickup.NetworkIdentity);
    }

    public override void OnServerPickupDestroyed(PickupDestroyedEventArgs ev)
    {
        if (_cfg.EnablePickupCulling && ev?.Pickup?.NetworkIdentity != null)
            ProximityCullingManager.UnregisterPickup(ev.Pickup.NetworkIdentity);
    }

    // ── Coroutine Driver Lifecycle ───────────────────────────────────────────

    private void StartLoop()
    {
        StopLoop();

        // Create a persistent GameObject that runs the coroutine.
        GameObject go = new("[MerOptimizer] CullingRunner");
        UnityEngine.Object.DontDestroyOnLoad(go);
        _runner = go.AddComponent<CullingRunner>();
        _runner.Begin(_cfg.CullUpdateInterval, _cfg.CullDistance);
    }

    private void StopLoop()
    {
        if (_runner != null)
        {
            UnityEngine.Object.Destroy(_runner.gameObject);
            _runner = null;
        }
    }
}

/// <summary>
/// MonoBehaviour that hosts the cull coroutine.
/// </summary>
internal sealed class CullingRunner : MonoBehaviour
{
    private float _interval;
    private float _distance;

    public void Begin(float interval, float distance)
    {
        _interval = interval;
        _distance = distance;
        StartCoroutine(CullCoroutine());
    }

    private System.Collections.IEnumerator CullCoroutine()
    {
        var wait = new WaitForSeconds(_interval);
        while (true)
        {
            yield return wait;
            try
            {
                ProximityCullingManager.RunCullCycle(_distance);
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error($"[MerOptimizer] CullLoop error: {ex}");
            }
        }
    }
}
