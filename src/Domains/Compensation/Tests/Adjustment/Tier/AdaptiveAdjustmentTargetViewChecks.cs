namespace Acme.Workforce.Compensation.Tests.Adjustment.Tier;

using Acme.Workforce.Compensation.Adjustment.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AdaptiveAdjustmentTargetViewChecks
{
    public static void AdaptiveAdjustmentTargetViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveAdjustmentTarget.NewAdaptiveAdjustmentTarget("code-180", "label-521");
        var view = AdaptiveAdjustmentTargetView.ToAdaptiveAdjustmentTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveAdjustmentTargetView.ToAdaptiveAdjustmentTargetViews(new List<AdaptiveAdjustmentTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveAdjustmentTargetViewShowsTheStatusInCapitals();
    }
}
