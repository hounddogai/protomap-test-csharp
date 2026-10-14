namespace Acme.Workforce.Compensation.Tests.Adjustment.AdjustmentRequest;

using Acme.Workforce.Compensation.Adjustment.AdjustmentRequest;

/// <summary>Checks of adjustment request code, run by the project's runner.</summary>
public static class AdjustmentTargetViewChecks
{
    public static void AdjustmentTargetViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentTarget.NewAdjustmentTarget("code-12", "label-137");
        var view = AdjustmentTargetView.ToAdjustmentTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentTargetView.ToAdjustmentTargetViews(new List<AdjustmentTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentTargetViewShowsTheStatusInCapitals();
    }
}
