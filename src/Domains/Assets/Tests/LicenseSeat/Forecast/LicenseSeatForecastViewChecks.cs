namespace Acme.Workforce.Assets.Tests.LicenseSeat.Forecast;

using Acme.Workforce.Assets.LicenseSeat.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LicenseSeatForecastViewChecks
{
    public static void LicenseSeatForecastViewShowsTheStatusInCapitals()
    {
        var item = LicenseSeatForecast.NewLicenseSeatForecast("code-769", "name-517");
        var view = LicenseSeatForecastView.ToLicenseSeatForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LicenseSeatForecastView.ToLicenseSeatForecastViews(new List<LicenseSeatForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LicenseSeatForecastViewShowsTheStatusInCapitals();
    }
}
