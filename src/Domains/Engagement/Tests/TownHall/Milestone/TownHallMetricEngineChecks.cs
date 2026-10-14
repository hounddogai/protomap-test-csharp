namespace Acme.Workforce.Engagement.Tests.TownHall.Milestone;

using Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class TownHallMetricEngineChecks
{
    public static void TownHallMetricEngineKeepsResultsWithinBounds()
    {
        var engine = TownHallMetricEngine.DefaultTownHallMetricEngine();
        var item = TownHallMetric.NewTownHallMetric("code-128", "title-815");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TownHallMetricEngineKeepsResultsWithinBounds();
    }
}
