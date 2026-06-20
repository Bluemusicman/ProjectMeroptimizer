using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;

namespace MerOptimizer;

internal sealed class SchematicCullingTracker : MonoBehaviour
{
    public List<NetworkIdentity> TrackedIdentities { get; } = [];

    private void OnDestroy()
    {
        ProximityCullingManager.Unregister(this);
    }
}
