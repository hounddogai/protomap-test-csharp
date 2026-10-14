namespace Acme.Workforce.Workplace.Tests.Visitor.Checkpoint;

using Acme.Workforce.Workplace.Visitor.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class SeasonalVisitorScheduleEngineChecks
{
    public static void SeasonalVisitorScheduleEngineKeepsResultsWithinBounds()
    {
        var engine = SeasonalVisitorScheduleEngine.DefaultSeasonalVisitorScheduleEngine();
        var item = SeasonalVisitorSchedule.NewSeasonalVisitorSchedule("code-318", "name-537");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SeasonalVisitorScheduleEngineKeepsResultsWithinBounds();
    }
}
