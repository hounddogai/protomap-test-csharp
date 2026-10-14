namespace Acme.Workforce.Payrollops.Tests.Garnishment.Forecast;

using Acme.Workforce.Payrollops.Garnishment.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class GarnishmentRollupEngineChecks
{
    public static void GarnishmentRollupEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentRollupEngine.DefaultGarnishmentRollupEngine();
        var item = GarnishmentRollup.NewGarnishmentRollup("code-256", "label-702");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentRollupEngineKeepsResultsWithinBounds();
    }
}
