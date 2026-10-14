namespace Acme.Workforce.Compensation.Tests.EquityGrant.Forecast;

using Acme.Workforce.Compensation.EquityGrant.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class EquityGrantForecastViewChecks
{
    public static void EquityGrantForecastViewShowsTheStatusInCapitals()
    {
        var item = EquityGrantForecast.NewEquityGrantForecast("code-966", "title-442");
        var view = EquityGrantForecastView.ToEquityGrantForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EquityGrantForecastView.ToEquityGrantForecastViews(new List<EquityGrantForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EquityGrantForecastViewShowsTheStatusInCapitals();
    }
}
