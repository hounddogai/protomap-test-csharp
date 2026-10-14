namespace Acme.Workforce.Absence.Tests.Accrual.Forecast;

using Acme.Workforce.Absence.Accrual.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class StrictAccrualForecastEngineChecks
{
    public static void StrictAccrualForecastEngineKeepsResultsWithinBounds()
    {
        var engine = StrictAccrualForecastEngine.DefaultStrictAccrualForecastEngine();
        var item = StrictAccrualForecast.NewStrictAccrualForecast("code-214", "name-613");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        StrictAccrualForecastEngineKeepsResultsWithinBounds();
    }
}
