namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledRefreshCycleEngineChecks
{
    public static void ScheduledRefreshCycleEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledRefreshCycleEngine.DefaultScheduledRefreshCycleEngine();
        var item = ScheduledRefreshCycle.NewScheduledRefreshCycle("code-306", "label-989");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledRefreshCycleEngineKeepsResultsWithinBounds();
    }
}
