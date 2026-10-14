namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class TieredRetroPayForecastViewChecks
{
    public static void TieredRetroPayForecastViewShowsTheStatusInCapitals()
    {
        var item = TieredRetroPayForecast.NewTieredRetroPayForecast("code-271", "name-817");
        var view = TieredRetroPayForecastView.ToTieredRetroPayForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredRetroPayForecastView.ToTieredRetroPayForecastViews(new List<TieredRetroPayForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredRetroPayForecastViewShowsTheStatusInCapitals();
    }
}
