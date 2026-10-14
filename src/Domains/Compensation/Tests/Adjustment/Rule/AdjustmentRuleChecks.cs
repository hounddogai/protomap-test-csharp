namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentRuleChecks
{
    public static void AdjustmentRuleStartsInNew()
    {
        var item = AdjustmentRule.NewAdjustmentRule("code-929", "label-810");
        Check.Equal(item.Status, AdjustmentRuleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentRuleMovesToQueued()
    {
        var item = AdjustmentRule.NewAdjustmentRule("code-929", "label-810");
        try
        {
            item.MoveTo(AdjustmentRuleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentRuleStatus.Queued, "status");
    }

    public static void AdjustmentRuleRejectsMovesFromFailed()
    {
        var item = AdjustmentRule.NewAdjustmentRule("code-929", "label-810");
        item.Status = AdjustmentRuleStatus.Failed;
        Check.Fails(() => item.MoveTo(AdjustmentRuleStatus.New), "move");
    }

    public static void RunAll()
    {
        AdjustmentRuleStartsInNew();
        AdjustmentRuleMovesToQueued();
        AdjustmentRuleRejectsMovesFromFailed();
    }
}
