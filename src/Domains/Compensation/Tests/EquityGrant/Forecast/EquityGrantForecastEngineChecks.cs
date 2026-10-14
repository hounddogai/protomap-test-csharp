namespace Acme.Workforce.Compensation.Tests.EquityGrant.Forecast;

using Acme.Workforce.Compensation.EquityGrant.Forecast;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class EquityGrantForecastEngineChecks
{
    public static void EquityGrantForecastEngineKeepsResultsWithinBounds()
    {
        var engine = EquityGrantForecastEngine.DefaultEquityGrantForecastEngine(Flags.NewRegistry());
        var item = EquityGrantForecast.NewEquityGrantForecast("code-966", "title-442");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        EquityGrantForecastEngineKeepsResultsWithinBounds();
    }
}
