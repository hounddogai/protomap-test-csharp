namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class BatchedAdjustmentRuleViewChecks
{
    public static void BatchedAdjustmentRuleViewShowsTheStatusInCapitals()
    {
        var item = BatchedAdjustmentRule.NewBatchedAdjustmentRule("code-196", "title-555");
        var view = BatchedAdjustmentRuleView.ToBatchedAdjustmentRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedAdjustmentRuleView.ToBatchedAdjustmentRuleViews(new List<BatchedAdjustmentRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedAdjustmentRuleViewShowsTheStatusInCapitals();
    }
}
