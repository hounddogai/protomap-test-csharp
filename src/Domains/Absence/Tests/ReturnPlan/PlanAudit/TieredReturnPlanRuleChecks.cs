namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class TieredReturnPlanRuleChecks
{
    public static void TieredReturnPlanRuleStartsInIdle()
    {
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("code-985", "name-451");
        Check.Equal(item.Status, TieredReturnPlanRuleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredReturnPlanRuleMovesToReady()
    {
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("code-985", "name-451");
        try
        {
            item.MoveTo(TieredReturnPlanRuleStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredReturnPlanRuleStatus.Ready, "status");
    }

    public static void TieredReturnPlanRuleRejectsMovesFromStopped()
    {
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("code-985", "name-451");
        item.Status = TieredReturnPlanRuleStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredReturnPlanRuleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredReturnPlanRuleStartsInIdle();
        TieredReturnPlanRuleMovesToReady();
        TieredReturnPlanRuleRejectsMovesFromStopped();
    }
}
