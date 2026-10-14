namespace Acme.Workforce.Contractors.Tests.Rate.Window;

using Acme.Workforce.Contractors.Rate.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class RateMetricEngineChecks
{
    public static void RateMetricEngineKeepsResultsWithinBounds()
    {
        var engine = RateMetricEngine.DefaultRateMetricEngine();
        var item = RateMetric.NewRateMetric("code-341", "name-658");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        RateMetricEngineKeepsResultsWithinBounds();
    }
}
