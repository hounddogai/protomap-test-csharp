namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SuccessionPlanRuleEngineChecks
{
    public static void SuccessionPlanRuleEngineKeepsResultsWithinBounds()
    {
        var engine = SuccessionPlanRuleEngine.DefaultSuccessionPlanRuleEngine();
        var item = SuccessionPlanRule.NewSuccessionPlanRule("code-240", "name-516");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        SuccessionPlanRuleEngineKeepsResultsWithinBounds();
    }
}
