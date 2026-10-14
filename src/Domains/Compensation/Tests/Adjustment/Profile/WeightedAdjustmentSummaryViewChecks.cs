namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class WeightedAdjustmentSummaryViewChecks
{
    public static void WeightedAdjustmentSummaryViewShowsTheStatusInCapitals()
    {
        var item = WeightedAdjustmentSummary.NewWeightedAdjustmentSummary("code-833", "label-490");
        var view = WeightedAdjustmentSummaryView.ToWeightedAdjustmentSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedAdjustmentSummaryView.ToWeightedAdjustmentSummaryViews(new List<WeightedAdjustmentSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedAdjustmentSummaryViewShowsTheStatusInCapitals();
    }
}
