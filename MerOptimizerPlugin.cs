global using System;
global using System.Collections.Generic;
global using System.Linq;

using HarmonyLib;
using LabApi.Events.CustomHandlers;
using LabApi.Loader.Features.Plugins;

namespace MerOptimizer;

/// <summary>
/// MerOptimizer — ProjectMER (LabAPI) performance plugin.
///
/// Optimizations applied:
///   • Proximity culling   — hides schematic primitives from distant players via Mirror observers.
///   • Collision stripping — removes Collidable flag from tiny/cosmetic primitives.
///   • Static forcing      — sets NetworkIsStatic on all non-animated primitives to eliminate sync cost.
///   • (batched spawning)  — scheduled for a future patch via Harmony on SchematicObject.CreateObject.
/// </summary>
public sealed class MerOptimizerPlugin : Plugin<Config>
{
    public static MerOptimizerPlugin Instance { get; private set; } = null!;

    private Harmony _harmony = null!;
    private CullingUpdateHandler _cullingHandler = null!;

    public override string Name        => "MerOptimizer";
    public override string Description => "Performance optimization plugin for ProjectMER (LabAPI). Reduces FPS drops from large schematics.";
    public override string Author      => "Souin";
    public override Version Version    => new(1, 0, 0, 0);

    // Require LabAPI 1.0+
    public override Version RequiredApiVersion => new(1, 0, 0, 0);

    public override void Enable()
    {
        Instance = this;

        _harmony = new Harmony($"meroptimizer.{DateTime.Now.Ticks}");
        _harmony.PatchAll(typeof(MerOptimizerPlugin).Assembly);

        _cullingHandler = new CullingUpdateHandler(Config!);
        CustomHandlersManager.RegisterEventsHandler(_cullingHandler);

        LabApi.Features.Console.Logger.Info("[MerOptimizer] Loaded! ProximityCulling=" +
            Config!.EnableProximityCulling +
            " CullDist=" + Config.CullDistance +
            " StaticForce=" + Config.ForceStaticOnNonAnimated +
            " CollisionStrip=" + Config.AutoStripTinyColliders);
    }

    public override void Disable()
    {
        CustomHandlersManager.UnregisterEventsHandler(_cullingHandler);
        _harmony.UnpatchAll(_harmony.Id);
        ProximityCullingManager.Clear();
        Instance = null!;
    }

    internal static void LogDebug(string msg)
    {
        if (Instance?.Config?.DebugStats == true)
            LabApi.Features.Console.Logger.Debug($"[MerOptimizer] {msg}");
    }
}
