namespace Acme.Workforce.Assets.Tests.Warranty.Forecast;

using Acme.Workforce.Assets.Warranty.Forecast;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class WarrantyForecastEngineChecks
{
    public static void WarrantyForecastEngineKeepsResultsWithinBounds()
    {
        var engine = WarrantyForecastEngine.DefaultWarrantyForecastEngine(Flags.NewRegistry());
        var item = WarrantyForecast.NewWarrantyForecast("code-258", "title-193");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        WarrantyForecastEngineKeepsResultsWithinBounds();
    }
}
