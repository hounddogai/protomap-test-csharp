namespace Acme.Workforce.Compensation.Tests.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

/// <summary>Checks of batched adjustment rule code, run by the project's runner.</summary>
public static class StrictAdjustmentBaselineViewChecks
{
    public static void StrictAdjustmentBaselineViewShowsTheStatusInCapitals()
    {
        var item = StrictAdjustmentBaseline.NewStrictAdjustmentBaseline("code-658", "name-803");
        var view = StrictAdjustmentBaselineView.ToStrictAdjustmentBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictAdjustmentBaselineView.ToStrictAdjustmentBaselineViews(new List<StrictAdjustmentBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictAdjustmentBaselineViewShowsTheStatusInCapitals();
    }
}
