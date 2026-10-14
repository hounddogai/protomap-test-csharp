namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Workplace.FloorPlan.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FloorPlanRuleEngineChecks
{
    public static void FloorPlanRuleEngineKeepsResultsWithinBounds()
    {
        var engine = FloorPlanRuleEngine.DefaultFloorPlanRuleEngine(Flags.NewRegistry());
        var item = FloorPlanRule.NewFloorPlanRule("code-920", "label-498");
        var result = engine.Evaluate(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        FloorPlanRuleEngineKeepsResultsWithinBounds();
    }
}
