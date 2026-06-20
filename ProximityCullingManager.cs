using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;

namespace MerOptimizer;

internal static class ProximityCullingManager
{
    private static readonly List<SchematicCullingTracker> _schematics = [];
    private static readonly HashSet<NetworkIdentity> _standaloneToys = [];
    private static readonly HashSet<NetworkIdentity> _pickups = [];

    private static readonly Dictionary<NetworkConnection, HashSet<uint>> _visibleFor = [];

    private static readonly Action<NetworkIdentity, NetworkConnection> ShowForConnectionDelegate;
    private static readonly Action<NetworkIdentity, NetworkConnection> HideForConnectionDelegate;

    static ProximityCullingManager()
    {
        var showMethod = typeof(NetworkServer).GetMethod("ShowForConnection", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (showMethod == null)
            throw new InvalidOperationException("Could not find NetworkServer.ShowForConnection method.");
        ShowForConnectionDelegate = (Action<NetworkIdentity, NetworkConnection>)Delegate.CreateDelegate(typeof(Action<NetworkIdentity, NetworkConnection>), showMethod);

        var hideMethod = typeof(NetworkServer).GetMethod("HideForConnection", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
        if (hideMethod == null)
            throw new InvalidOperationException("Could not find NetworkServer.HideForConnection method.");
        HideForConnectionDelegate = (Action<NetworkIdentity, NetworkConnection>)Delegate.CreateDelegate(typeof(Action<NetworkIdentity, NetworkConnection>), hideMethod);
    }

    public static void Register(SchematicCullingTracker tracker) => _schematics.Add(tracker);

    public static void Unregister(SchematicCullingTracker tracker)
    {
        _schematics.Remove(tracker);
        foreach (uint netId in tracker.TrackedIdentities.Select(ni => ni.netId))
        {
            foreach (HashSet<uint> set in _visibleFor.Values)
                set.Remove(netId);
        }
    }

    public static void RegisterStandaloneToy(NetworkIdentity netId)
    {
        if (netId != null)
            _standaloneToys.Add(netId);
    }

    public static void UnregisterStandaloneToy(NetworkIdentity netId)
    {
        if (netId != null)
        {
            _standaloneToys.Remove(netId);
            foreach (HashSet<uint> set in _visibleFor.Values)
                set.Remove(netId.netId);
        }
    }

    public static void RegisterPickup(NetworkIdentity netId)
    {
        if (netId != null)
            _pickups.Add(netId);
    }

    public static void UnregisterPickup(NetworkIdentity netId)
    {
        if (netId != null)
        {
            _pickups.Remove(netId);
            foreach (HashSet<uint> set in _visibleFor.Values)
                set.Remove(netId.netId);
        }
    }

    public static void Clear()
    {
        _schematics.Clear();
        _standaloneToys.Clear();
        _pickups.Clear();
        _visibleFor.Clear();
    }

    public static void RunCullCycle(float cullDistance)
    {
        Config cfg = MerOptimizerPlugin.Instance.Config;

        if (_schematics.Count == 0 && _standaloneToys.Count == 0 && _pickups.Count == 0) return;

        float sqrCull = cullDistance * cullDistance;
        float sqrPickupCull = cfg.PickupCullDistance * cfg.PickupCullDistance;

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn?.identity == null) continue;

            Vector3 playerPos = conn.identity.transform.position;

            if (!_visibleFor.TryGetValue(conn, out HashSet<uint>? visible))
            {
                visible = [];
                _visibleFor[conn] = visible;
            }

            if (cfg.EnableProximityCulling && _schematics.Count > 0)
            {
                foreach (SchematicCullingTracker schematic in _schematics)
                {
                    if (schematic == null || schematic.gameObject == null) continue;

                    Vector3 schematicPos = schematic.transform.position;
                    bool withinRange = (playerPos - schematicPos).sqrMagnitude <= sqrCull;

                    foreach (NetworkIdentity netId in schematic.TrackedIdentities)
                    {
                        if (netId == null) continue;
                        CullObject(conn, netId, visible, withinRange);
                    }
                }
            }

            if (cfg.EnableStandaloneToyCulling && _standaloneToys.Count > 0)
            {
                foreach (NetworkIdentity netId in _standaloneToys)
                {
                    if (netId == null) continue;
                    Vector3 toyPos = netId.transform.position;
                    bool withinRange = (playerPos - toyPos).sqrMagnitude <= sqrCull;
                    CullObject(conn, netId, visible, withinRange);
                }
            }

            if (cfg.EnablePickupCulling && _pickups.Count > 0)
            {
                foreach (NetworkIdentity netId in _pickups)
                {
                    if (netId == null) continue;
                    Vector3 pickupPos = netId.transform.position;
                    bool withinRange = (playerPos - pickupPos).sqrMagnitude <= sqrPickupCull;
                    CullObject(conn, netId, visible, withinRange);
                }
            }
        }

        List<NetworkConnection> dead = _visibleFor.Keys
            .Where(c => c == null || !NetworkServer.connections.ContainsValue(c as NetworkConnectionToClient))
            .ToList();
        foreach (NetworkConnection c in dead)
            _visibleFor.Remove(c);
    }

    private static void CullObject(NetworkConnectionToClient conn, NetworkIdentity netId, HashSet<uint> visible, bool withinRange)
    {
        bool currentlyVisible = visible.Contains(netId.netId);

        if (withinRange && !currentlyVisible)
        {
            ShowForConnection(conn, netId);
            visible.Add(netId.netId);
        }
        else if (!withinRange && currentlyVisible)
        {
            HideForConnection(conn, netId);
            visible.Remove(netId.netId);
        }
    }

    private static void ShowForConnection(NetworkConnectionToClient conn, NetworkIdentity identity)
    {
        try
        {
            ShowForConnectionDelegate(identity, conn);
        }
        catch (Exception ex)
        {
            MerOptimizerPlugin.LogDebug($"[CullShow] {ex.Message}");
        }
    }

    private static void HideForConnection(NetworkConnectionToClient conn, NetworkIdentity identity)
    {
        try
        {
            HideForConnectionDelegate(identity, conn);
        }
        catch (Exception ex)
        {
            MerOptimizerPlugin.LogDebug($"[CullHide] {ex.Message}");
        }
    }
}
