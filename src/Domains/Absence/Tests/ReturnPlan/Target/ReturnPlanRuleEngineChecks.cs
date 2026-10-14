namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanRuleEngineChecks
{
    public static void ReturnPlanRuleEngineKeepsResultsWithinBounds()
    {
        var engine = ReturnPlanRuleEngine.DefaultReturnPlanRuleEngine(Flags.NewRegistry());
        var item = ReturnPlanRule.NewReturnPlanRule("code-751", "label-518");
        var result = engine.Price(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        ReturnPlanRuleEngineKeepsResultsWithinBounds();
    }
}
