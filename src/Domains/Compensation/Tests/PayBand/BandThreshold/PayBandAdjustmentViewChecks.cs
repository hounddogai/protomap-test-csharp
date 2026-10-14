namespace Acme.Workforce.Compensation.Tests.PayBand.BandThreshold;

using Acme.Workforce.Compensation.PayBand.BandThreshold;

/// <summary>Checks of band threshold code, run by the project's runner.</summary>
public static class PayBandAdjustmentViewChecks
{
    public static void PayBandAdjustmentViewShowsTheStatusInCapitals()
    {
        var item = PayBandAdjustment.NewPayBandAdjustment("code-156", "label-798");
        var view = PayBandAdjustmentView.ToPayBandAdjustmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandAdjustmentView.ToPayBandAdjustmentViews(new List<PayBandAdjustment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandAdjustmentViewShowsTheStatusInCapitals();
    }
}
