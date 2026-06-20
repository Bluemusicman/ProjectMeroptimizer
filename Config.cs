using YamlDotNet.Serialization;

namespace MerOptimizer;

/// <summary>
/// Configuration for MerOptimizer. Saved to config.yml automatically by LabAPI.
/// </summary>
public class Config
{
    public bool IsEnabled { get; set; } = true;

    // ── Proximity Culling ─────────────────────────────────────────────────────

    /// <summary>Enables per-player proximity visibility culling for schematic primitives.</summary>
    [YamlMember(Description = "Enables per-player proximity culling. Hides schematic primitives from players who are too far away.")]
    public bool EnableProximityCulling { get; set; } = true;

    /// <summary>Distance in metres beyond which primitives are hidden from that player.</summary>
    [YamlMember(Description = "Distance in metres. Primitives hidden from players beyond this range.")]
    public float CullDistance { get; set; } = 60f;

    /// <summary>Enables per-player proximity visibility culling for standalone toys (not part of schematics).</summary>
    [YamlMember(Description = "Enables proximity culling for standalone (admin/plugin spawned) toys.")]
    public bool EnableStandaloneToyCulling { get; set; } = true;

    /// <summary>Enables per-player proximity visibility culling for dropped items (Pickups) on the ground.</summary>
    [YamlMember(Description = "Enables proximity culling for dropped items on the ground (greatly improves FPS in item-heavy areas).")]
    public bool EnablePickupCulling { get; set; } = true;

    /// <summary>Distance in metres beyond which dropped items are hidden from that player.</summary>
    [YamlMember(Description = "Distance in metres. Dropped items hidden from players beyond this range.")]
    public float PickupCullDistance { get; set; } = 35f;

    /// <summary>How often (in seconds) to re-evaluate visibility for each player.</summary>
    [YamlMember(Description = "Interval in seconds between each culling update per player.")]
    public float CullUpdateInterval { get; set; } = 1.0f;

    // ── Collision Optimisation ────────────────────────────────────────────────

    /// <summary>Automatically strips the Collidable flag from very small primitives.</summary>
    [YamlMember(Description = "Auto-strips the Collidable flag from tiny cosmetic primitives (saves server physics CPU).")]
    public bool AutoStripTinyColliders { get; set; } = true;

    /// <summary>Max scale axis value below which a primitive is considered 'tiny' and gets collisions removed.</summary>
    [YamlMember(Description = "Primitives whose largest scale axis is smaller than this value lose their collision flag.")]
    public float TinyColliderMaxScale { get; set; } = 0.15f;

    // ── Batched Spawning ──────────────────────────────────────────────────────

    /// <summary>Spreads schematic block spawning across multiple frames to avoid tick-rate spikes.</summary>
    [YamlMember(Description = "Spreads large schematic spawns over multiple server frames instead of one massive tick.")]
    public bool EnableBatchedSpawning { get; set; } = true;

    /// <summary>Number of network objects to register per server frame during batched spawning.</summary>
    [YamlMember(Description = "How many objects to spawn per frame when BatchedSpawning is enabled.")]
    public int BatchSize { get; set; } = 30;

    /// <summary>Frame delay between each batch.</summary>
    [YamlMember(Description = "Frames to wait between batches. 1 = every frame, 2 = every other frame, etc.")]
    public int BatchFrameDelay { get; set; } = 1;

    // ── Static Object Optimisation ────────────────────────────────────────────

    /// <summary>Force NetworkIsStatic = true on all schematic primitives that are not animated.</summary>
    [YamlMember(Description = "Forces NetworkIsStatic on non-animated schematic primitives. Eliminates per-frame transform sync cost.")]
    public bool ForceStaticOnNonAnimated { get; set; } = true;

    // ── Debug ─────────────────────────────────────────────────────────────────

    [YamlMember(Description = "Print optimization statistics to the console on each round start.")]
    public bool DebugStats { get; set; } = false;
}
