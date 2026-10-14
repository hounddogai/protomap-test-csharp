namespace Acme.Workforce.Compensation.Tests.Adjustment.Schedule;

using Acme.Workforce.Compensation.Adjustment.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class AdjustmentVarianceViewChecks
{
    public static void AdjustmentVarianceViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentVariance.NewAdjustmentVariance("code-212", "label-625");
        var view = AdjustmentVarianceView.ToAdjustmentVarianceView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentVarianceView.ToAdjustmentVarianceViews(new List<AdjustmentVariance> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentVarianceViewShowsTheStatusInCapitals();
    }
}
