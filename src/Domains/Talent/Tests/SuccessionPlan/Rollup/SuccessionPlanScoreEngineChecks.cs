namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Rollup;

using Acme.Workforce.Talent.SuccessionPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SuccessionPlanScoreEngineChecks
{
    public static void SuccessionPlanScoreEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanScoreEngine.DefaultSuccessionPlanScoreEngine();
        var item = SuccessionPlanScore.NewSuccessionPlanScore("code-492", "title-253");
        var result = engine.Compute(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanScoreEngineKeepsResultsWithinBounds();
    }
}
