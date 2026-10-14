namespace Acme.Workforce.Engagement.Tests.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ActionPlanRuleChecks
{
    public static void ActionPlanRuleStartsInCreated()
    {
        var item = ActionPlanRule.NewActionPlanRule("code-961", "name-670");
        Check.Equal(item.Status, ActionPlanRuleStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ActionPlanRuleMovesToRetired()
    {
        var item = ActionPlanRule.NewActionPlanRule("code-961", "name-670");
        try
        {
            item.MoveTo(ActionPlanRuleStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ActionPlanRuleStatus.Retired, "status");
    }

    public static void ActionPlanRuleRejectsMovesFromRetired()
    {
        var item = ActionPlanRule.NewActionPlanRule("code-961", "name-670");
        item.Status = ActionPlanRuleStatus.Retired;
        Check.Fails(() => item.MoveTo(ActionPlanRuleStatus.Created), "move");
    }

    public static void RunAll()
    {
        ActionPlanRuleStartsInCreated();
        ActionPlanRuleMovesToRetired();
        ActionPlanRuleRejectsMovesFromRetired();
    }
}
