namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RefreshForecastEngineChecks
{
    public static void RefreshForecastEngineKeepsResultsWithinBounds()
    {
        var engine = RefreshForecastEngine.DefaultRefreshForecastEngine();
        var item = RefreshForecast.NewRefreshForecast("code-393", "title-741");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RefreshForecastEngineKeepsResultsWithinBounds();
    }
}
