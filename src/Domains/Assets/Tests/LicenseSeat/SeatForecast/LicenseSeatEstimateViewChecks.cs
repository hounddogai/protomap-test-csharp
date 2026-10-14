namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class LicenseSeatEstimateViewChecks
{
    public static void LicenseSeatEstimateViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatEstimate.NewLicenseSeatEstimate("code-422", "title-761");
        var view = LicenseSeatEstimateView.ToLicenseSeatEstimateView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatEstimateView.ToLicenseSeatEstimateViews(new List<LicenseSeatEstimate> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatEstimateViewShowsTheStatusInCapitals();
    }
}
