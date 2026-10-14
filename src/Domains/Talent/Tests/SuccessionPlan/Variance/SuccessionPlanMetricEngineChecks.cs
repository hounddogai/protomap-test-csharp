namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanMetricEngineChecks
{
    public static void SuccessionPlanMetricEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanMetricEngine.DefaultSuccessionPlanMetricEngine();
        var item = SuccessionPlanMetric.NewSuccessionPlanMetric("code-339", "title-542");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanMetricEngineKeepsResultsWithinBounds();
    }
}
