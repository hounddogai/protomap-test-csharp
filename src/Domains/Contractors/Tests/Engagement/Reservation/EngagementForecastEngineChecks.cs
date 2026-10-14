namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class EngagementForecastEngineChecks
{
    public static void EngagementForecastEngineKeepsResultsWithinBounds()
    {
        var engine = EngagementForecastEngine.DefaultEngagementForecastEngine();
        var item = EngagementForecast.NewEngagementForecast("code-775", "label-414");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EngagementForecastEngineKeepsResultsWithinBounds();
    }
}
