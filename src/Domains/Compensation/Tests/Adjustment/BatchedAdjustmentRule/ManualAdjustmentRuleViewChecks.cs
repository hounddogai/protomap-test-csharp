namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class ManualAdjustmentRuleViewChecks
{
    public static void ManualAdjustmentRuleViewShowsTheStatusInCapitals()
    {
        var item = ManualAdjustmentRule.NewManualAdjustmentRule("code-770", "title-367");
        var view = ManualAdjustmentRuleView.ToManualAdjustmentRuleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualAdjustmentRuleView.ToManualAdjustmentRuleViews(new List<ManualAdjustmentRule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualAdjustmentRuleViewShowsTheStatusInCapitals();
    }
}
