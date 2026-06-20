using YamlDotNet.Serialization;

namespace MerOptimizer;

public class Config
{
    public bool IsEnabled { get; set; } = true;

    [YamlMember(Description = "Enables per-player proximity culling. Hides schematic primitives from players who are too far away.")]
    public bool EnableProximityCulling { get; set; } = true;

    [YamlMember(Description = "Distance in metres. Primitives hidden from players beyond this range.")]
    public float CullDistance { get; set; } = 60f;

    [YamlMember(Description = "Enables proximity culling for standalone (admin/plugin spawned) toys.")]
    public bool EnableStandaloneToyCulling { get; set; } = true;

    [YamlMember(Description = "Enables proximity culling for dropped items on the ground (greatly improves FPS in item-heavy areas).")]
    public bool EnablePickupCulling { get; set; } = true;

    [YamlMember(Description = "Distance in metres. Dropped items hidden from players beyond this range.")]
    public float PickupCullDistance { get; set; } = 35f;

    [YamlMember(Description = "Interval in seconds between each culling update per player.")]
    public float CullUpdateInterval { get; set; } = 1.0f;

    [YamlMember(Description = "Auto-strips the Collidable flag from tiny cosmetic primitives (saves server physics CPU).")]
    public bool AutoStripTinyColliders { get; set; } = true;

    [YamlMember(Description = "Primitives whose largest scale axis is smaller than this value lose their collision flag.")]
    public float TinyColliderMaxScale { get; set; } = 0.15f;

    [YamlMember(Description = "Spreads large schematic spawns over multiple server frames instead of one massive tick.")]
    public bool EnableBatchedSpawning { get; set; } = true;

    [YamlMember(Description = "How many objects to spawn per frame when BatchedSpawning is enabled.")]
    public int BatchSize { get; set; } = 30;

    [YamlMember(Description = "Frames to wait between batches. 1 = every frame, 2 = every other frame, etc.")]
    public int BatchFrameDelay { get; set; } = 1;

    [YamlMember(Description = "Forces NetworkIsStatic on non-animated schematic primitives. Eliminates per-frame transform sync cost.")]
    public bool ForceStaticOnNonAnimated { get; set; } = true;

    [YamlMember(Description = "Print optimization statistics to the console on each round start.")]
    public bool DebugStats { get; set; } = false;
}
