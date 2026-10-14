namespace Acme.Workforce.Compensation.Tests.PayBand.BandTier;

using Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>Checks of band tier code, run by the project's runner.</summary>
public static class PendingPayBandTierViewChecks
{
    public static void PendingPayBandTierViewShowsTheStatusInCapitals()
    {
        var item = PendingPayBandTier.NewPendingPayBandTier("code-131", "label-985");
        var view = PendingPayBandTierView.ToPendingPayBandTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingPayBandTierView.ToPendingPayBandTierViews(new List<PendingPayBandTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingPayBandTierViewShowsTheStatusInCapitals();
    }
}
