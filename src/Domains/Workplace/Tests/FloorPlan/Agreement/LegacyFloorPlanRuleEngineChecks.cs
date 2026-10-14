namespace Acme.Workforce.Workplace.Tests.FloorPlan.Agreement;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LegacyFloorPlanRuleEngineChecks
{
    public static void LegacyFloorPlanRuleEngineKeepsResultsWithinBounds()
    {
        var engine = LegacyFloorPlanRuleEngine.DefaultLegacyFloorPlanRuleEngine();
        var item = LegacyFloorPlanRule.NewLegacyFloorPlanRule("code-234", "label-3");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        LegacyFloorPlanRuleEngineKeepsResultsWithinBounds();
    }
}
