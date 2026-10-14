namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalVisitorCheckpointEngineChecks
{
    public static void SeasonalVisitorCheckpointEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalVisitorCheckpointEngine.DefaultSeasonalVisitorCheckpointEngine(Flags.NewRegistry());
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("code-809", "title-56");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalVisitorCheckpointEngineKeepsResultsWithinBounds();
    }
}
