namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRule;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

/// <summary>Checks of adjustment rule code, run by the project's runner.</summary>
public static class AdaptiveAdjustmentNoticeViewChecks
{
    public static void AdaptiveAdjustmentNoticeViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveAdjustmentNotice.NewAdaptiveAdjustmentNotice("code-509", "name-395");
        var view = AdaptiveAdjustmentNoticeView.ToAdaptiveAdjustmentNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveAdjustmentNoticeView.ToAdaptiveAdjustmentNoticeViews(new List<AdaptiveAdjustmentNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveAdjustmentNoticeViewShowsTheStatusInCapitals();
    }
}
