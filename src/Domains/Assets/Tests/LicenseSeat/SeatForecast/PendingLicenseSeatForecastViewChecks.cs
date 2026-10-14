namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatForecast;

using Acme.Workforce.Assets.LicenseSeat.SeatForecast;

/// <summary>Checks of seat forecast code, run by the project's runner.</summary>
public static class PendingLicenseSeatForecastViewChecks
{
    public static void PendingLicenseSeatForecastViewShowsTheStatusInCapitals()
    {
        var item = PendingLicenseSeatForecast.NewPendingLicenseSeatForecast("code-859", "title-746");
        var view = PendingLicenseSeatForecastView.ToPendingLicenseSeatForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PendingLicenseSeatForecastView.ToPendingLicenseSeatForecastViews(new List<PendingLicenseSeatForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PendingLicenseSeatForecastViewShowsTheStatusInCapitals();
    }
}
