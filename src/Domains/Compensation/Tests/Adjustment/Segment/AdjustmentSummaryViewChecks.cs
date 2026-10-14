namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentSummaryViewChecks
{
    public static void AdjustmentSummaryViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentSummary.NewAdjustmentSummary("code-90", "label-148");
        var view = AdjustmentSummaryView.ToAdjustmentSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentSummaryView.ToAdjustmentSummaryViews(new List<AdjustmentSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentSummaryViewShowsTheStatusInCapitals();
    }
}
