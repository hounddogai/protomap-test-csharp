namespace Acme.Workforce.Compensation.Tests.PayBand.Reservation;

using Acme.Workforce.Compensation.PayBand.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class TieredPayBandEstimateViewChecks
{
    public static void TieredPayBandEstimateViewShowsTheStatusInCapitals()
    {
        var item = TieredPayBandEstimate.NewTieredPayBandEstimate("code-627", "label-771");
        var view = TieredPayBandEstimateView.ToTieredPayBandEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPayBandEstimateView.ToTieredPayBandEstimateViews(new List<TieredPayBandEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPayBandEstimateViewShowsTheStatusInCapitals();
    }
}
