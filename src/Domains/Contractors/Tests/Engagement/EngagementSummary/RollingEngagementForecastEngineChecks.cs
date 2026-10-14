namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class RollingEngagementForecastEngineChecks
{
    public static void RollingEngagementForecastEngineKeepsResultsWithinBounds()
    {
        var engine = RollingEngagementForecastEngine.DefaultRollingEngagementForecastEngine();
        var item = RollingEngagementForecast.NewRollingEngagementForecast("code-902", "label-83");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RollingEngagementForecastEngineKeepsResultsWithinBounds();
    }
}
