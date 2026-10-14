namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;
using Acme.Workforce.Kit.Flags;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class TieredReturnPlanRuleEngineChecks
{
    public static void TieredReturnPlanRuleEngineKeepsResultsWithinBounds()
    {
        var engine = TieredReturnPlanRuleEngine.DefaultTieredReturnPlanRuleEngine(Flags.NewRegistry());
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("code-985", "name-451");
        var result = engine.Run(item);
        Check.True(result >= engine.Floor && result <= engine.Ceiling, "result out of bounds");
        Check.True(engine.Trace.Count >= 0, "trace");
    }

    public static void RunAll()
    {
        TieredReturnPlanRuleEngineKeepsResultsWithinBounds();
    }
}
