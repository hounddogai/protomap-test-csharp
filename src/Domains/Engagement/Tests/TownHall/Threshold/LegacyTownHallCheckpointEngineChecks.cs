namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyTownHallCheckpointEngineChecks
{
    public static void LegacyTownHallCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyTownHallCheckpointEngine.DefaultLegacyTownHallCheckpointEngine(Flags.NewRegistry());
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("code-168", "label-406");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyTownHallCheckpointEngineKeepsResultsWithinBounds();
    }
}
