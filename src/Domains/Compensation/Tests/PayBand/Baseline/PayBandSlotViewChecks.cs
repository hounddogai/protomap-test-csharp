namespace Acme.Workforce.Compensation.Tests.PayBand.Baseline;

using Acme.Workforce.Compensation.PayBand.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class PayBandSlotViewChecks
{
    public static void PayBandSlotViewShowsTheStatusInCapitals()
    {
        var item = PayBandSlot.NewPayBandSlot("code-131", "title-747");
        var view = PayBandSlotView.ToPayBandSlotView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PayBandSlotView.ToPayBandSlotViews(new List<PayBandSlot> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PayBandSlotViewShowsTheStatusInCapitals();
    }
}
