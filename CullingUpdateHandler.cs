using LabApi.Events.Arguments.ServerEvents;
using LabApi.Events.CustomHandlers;
using MEC;

namespace MerOptimizer;

/// <summary>
/// Drives the periodic proximity-culling update loop and handles item pickup lifecycle events.
/// Uses MEC (More Effective Coroutines) which is bundled with the game assembly.
/// </summary>
internal sealed class CullingUpdateHandler : CustomEventsHandler
{
    private CoroutineHandle _cullHandle;
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

    // ── MEC Coroutine Lifecycle ──────────────────────────────────────────────

    private void StartLoop()
    {
        StopLoop();
        _cullHandle = Timing.RunCoroutine(CullCoroutine());
    }

    private void StopLoop()
    {
        Timing.KillCoroutines(_cullHandle);
    }

    private IEnumerator<float> CullCoroutine()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(_cfg.CullUpdateInterval);
            try
            {
                ProximityCullingManager.RunCullCycle(_cfg.CullDistance);
            }
            catch (Exception ex)
            {
                LabApi.Features.Console.Logger.Error($"[MerOptimizer] CullLoop error: {ex}");
            }
        }
    }
}
