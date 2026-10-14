namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AdjustmentThresholdViewChecks
{
    public static void AdjustmentThresholdViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentThreshold.NewAdjustmentThreshold("code-889", "label-938");
        var view = AdjustmentThresholdView.ToAdjustmentThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentThresholdView.ToAdjustmentThresholdViews(new List<AdjustmentThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentThresholdViewShowsTheStatusInCapitals();
    }
}
