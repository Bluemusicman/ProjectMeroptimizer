using AdminToys;
using LabApi.Features.Wrappers;
using Mirror;
using UnityEngine;

namespace MerOptimizer;

/// <summary>
/// Tracks which NetworkIdentities are managed by MerOptimizer for proximity culling.
/// Attached as a MonoBehaviour to the schematic root GameObject so it is cleaned up
/// automatically when ProjectMER destroys the schematic.
/// </summary>
internal sealed class SchematicCullingTracker : MonoBehaviour
{
    /// <summary>All networked primitives/lights that belong to this schematic.</summary>
    public List<NetworkIdentity> TrackedIdentities { get; } = [];

    private void OnDestroy()
    {
        // Remove from the global registry when the schematic is cleaned up.
        ProximityCullingManager.Unregister(this);
    }
}
