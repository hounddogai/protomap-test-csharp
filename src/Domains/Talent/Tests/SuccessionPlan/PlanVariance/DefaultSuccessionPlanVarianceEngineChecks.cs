namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Talent.SuccessionPlan.PlanVariance;

/// <summary>Checks of plan variance code, run by the project's runner.</summary>
public static class DefaultSuccessionPlanVarianceEngineChecks
{
    public static void DefaultSuccessionPlanVarianceEngineKeepsResultsWithinBounds()
    {
        var engine = DefaultSuccessionPlanVarianceEngine.DefaultDefaultSuccessionPlanVarianceEngine(Flags.NewRegistry());
        var item = DefaultSuccessionPlanVariance.NewDefaultSuccessionPlanVariance("code-263", "title-319");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        DefaultSuccessionPlanVarianceEngineKeepsResultsWithinBounds();
    }
}
