namespace Acme.Workforce.Payrollops.Tests.Garnishment.Plan;

using Acme.Workforce.Payrollops.Garnishment.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class GarnishmentMetricEngineChecks
{
    public static void GarnishmentMetricEngineKeepsResultsWithinBounds()
    {
        var engine = GarnishmentMetricEngine.DefaultGarnishmentMetricEngine();
        var item = GarnishmentMetric.NewGarnishmentMetric("code-796", "name-986");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        GarnishmentMetricEngineKeepsResultsWithinBounds();
    }
}
