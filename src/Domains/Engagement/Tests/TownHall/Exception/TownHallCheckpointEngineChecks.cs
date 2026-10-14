namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallCheckpointEngineChecks
{
    public static void TownHallCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallCheckpointEngine.DefaultTownHallCheckpointEngine(Flags.NewRegistry());
        var item = TownHallCheckpoint.NewTownHallCheckpoint("code-269", "title-25");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallCheckpointEngineKeepsResultsWithinBounds();
    }
}
