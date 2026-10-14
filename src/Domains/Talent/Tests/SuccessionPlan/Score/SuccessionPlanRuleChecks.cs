namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Score;

using Acme.Workforce.Talent.SuccessionPlan.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SuccessionPlanRuleChecks
{
    public static void SuccessionPlanRuleStartsInNew()
    {
        var item = SuccessionPlanRule.NewSuccessionPlanRule("code-240", "name-516");
        Check.Equal(item.Status, SuccessionPlanRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SuccessionPlanRuleMovesToQueued()
    {
        var item = SuccessionPlanRule.NewSuccessionPlanRule("code-240", "name-516");
        try
        {
            item.MoveTo(SuccessionPlanRuleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SuccessionPlanRuleStatus.Queued, "status");
    }

    public static void SuccessionPlanRuleRejectsMovesFromFailed()
    {
        var item = SuccessionPlanRule.NewSuccessionPlanRule("code-240", "name-516");
        item.Status = SuccessionPlanRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(SuccessionPlanRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        SuccessionPlanRuleStartsInNew();
        SuccessionPlanRuleMovesToQueued();
        SuccessionPlanRuleRejectsMovesFromFailed();
    }
}
