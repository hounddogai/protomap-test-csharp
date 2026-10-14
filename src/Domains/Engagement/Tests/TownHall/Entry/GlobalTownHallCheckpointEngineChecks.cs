namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalTownHallCheckpointEngineChecks
{
    public static void GlobalTownHallCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = GlobalTownHallCheckpointEngine.DefaultGlobalTownHallCheckpointEngine(Flags.NewRegistry());
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("code-933", "label-190");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GlobalTownHallCheckpointEngineKeepsResultsWithinBounds();
    }
}
