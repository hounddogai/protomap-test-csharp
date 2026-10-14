namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class GlobalAdjustmentRuleChecks
{
    public static void GlobalAdjustmentRuleStartsInOpen()
    {
        var item = GlobalAdjustmentRule.NewGlobalAdjustmentRule("code-441", "label-171");
        Check.Equal(item.Status, GlobalAdjustmentRuleStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalAdjustmentRuleMovesToInReview()
    {
        var item = GlobalAdjustmentRule.NewGlobalAdjustmentRule("code-441", "label-171");
        try
        {
            item.MoveTo(GlobalAdjustmentRuleStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalAdjustmentRuleStatus.InReview, "status");
    }

    public static void GlobalAdjustmentRuleRejectsMovesFromArchived()
    {
        var item = GlobalAdjustmentRule.NewGlobalAdjustmentRule("code-441", "label-171");
        item.Status = GlobalAdjustmentRuleStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalAdjustmentRuleStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalAdjustmentRuleStartsInOpen();
        GlobalAdjustmentRuleMovesToInReview();
        GlobalAdjustmentRuleRejectsMovesFromArchived();
    }
}
