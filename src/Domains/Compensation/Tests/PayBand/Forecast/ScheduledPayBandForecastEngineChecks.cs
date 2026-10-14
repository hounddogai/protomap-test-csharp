namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledPayBandForecastEngineChecks
{
    public static void ScheduledPayBandForecastEngineKeepsResultsWithinBounds()
    {
        var engine = ScheduledPayBandForecastEngine.DefaultScheduledPayBandForecastEngine();
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("code-669", "label-428");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ScheduledPayBandForecastEngineKeepsResultsWithinBounds();
    }
}
