namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanRuleChecks
{
    public static void ReturnPlanRuleStartsInPlanned()
    {
        var item = ReturnPlanRule.NewReturnPlanRule("code-751", "label-518");
        Check.Equal(item.Status, ReturnPlanRuleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReturnPlanRuleMovesToCancelled()
    {
        var item = ReturnPlanRule.NewReturnPlanRule("code-751", "label-518");
        try
        {
            item.MoveTo(ReturnPlanRuleStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReturnPlanRuleStatus.Cancelled, "status");
    }

    public static void ReturnPlanRuleRejectsMovesFromCancelled()
    {
        var item = ReturnPlanRule.NewReturnPlanRule("code-751", "label-518");
        item.Status = ReturnPlanRuleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ReturnPlanRuleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ReturnPlanRuleStartsInPlanned();
        ReturnPlanRuleMovesToCancelled();
        ReturnPlanRuleRejectsMovesFromCancelled();
    }
}
