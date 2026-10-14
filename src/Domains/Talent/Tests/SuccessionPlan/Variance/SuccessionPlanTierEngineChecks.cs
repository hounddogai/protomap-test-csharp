namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Variance;

using Acme.Workforce.Talent.SuccessionPlan.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class SuccessionPlanTierEngineChecks
{
    public static void SuccessionPlanTierEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanTierEngine.DefaultSuccessionPlanTierEngine();
        var item = SuccessionPlanTier.NewSuccessionPlanTier("code-292", "title-27");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanTierEngineKeepsResultsWithinBounds();
    }
}
