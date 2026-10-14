namespace Acme.Workforce.Compensation.Tests.Adjustment.Summary;

using Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class GlobalAdjustmentSummaryViewChecks
{
    public static void GlobalAdjustmentSummaryViewShowsTheStatusInCapitals()
    {
        var item = GlobalAdjustmentSummary.NewGlobalAdjustmentSummary("code-867", "title-524");
        var view = GlobalAdjustmentSummaryView.ToGlobalAdjustmentSummaryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(GlobalAdjustmentSummaryView.ToGlobalAdjustmentSummaryViews(new List<GlobalAdjustmentSummary> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        GlobalAdjustmentSummaryViewShowsTheStatusInCapitals();
    }
}
