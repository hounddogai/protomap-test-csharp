namespace Acme.Workforce.Payrollops.Tests.RetroPay.Forecast;

using Acme.Workforce.Payrollops.RetroPay.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RetroPayForecastViewChecks
{
    public static void RetroPayForecastViewShowsTheStatusInCapitals()
    {
        var item = RetroPayForecast.NewRetroPayForecast("code-777", "title-1");
        var view = RetroPayForecastView.ToRetroPayForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RetroPayForecastView.ToRetroPayForecastViews(new List<RetroPayForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RetroPayForecastViewShowsTheStatusInCapitals();
    }
}
