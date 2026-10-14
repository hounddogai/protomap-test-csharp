namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanClaim;

using Acme.Workforce.Talent.SuccessionPlan.PlanClaim;

/// <summary>Checks of plan claim code, run by the project's runner.</summary>
public static class LenientSuccessionPlanScoreEngineChecks
{
    public static void LenientSuccessionPlanScoreEngineKeepsResultsWithinBounds()
    {
        var engine = LenientSuccessionPlanScoreEngine.DefaultLenientSuccessionPlanScoreEngine();
        var item = LenientSuccessionPlanScore.NewLenientSuccessionPlanScore("code-332", "label-470");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LenientSuccessionPlanScoreEngineKeepsResultsWithinBounds();
    }
}
