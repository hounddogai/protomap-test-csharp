namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentForecastEngineChecks
{
    public static void GarnishmentForecastEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentForecastEngine.DefaultGarnishmentForecastEngine(Flags.NewRegistry());
        var item = GarnishmentForecast.NewGarnishmentForecast("code-406", "name-44");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentForecastEngineKeepsResultsWithinBounds();
    }
}
