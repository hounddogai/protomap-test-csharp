namespace Acme.Workforce.Assets.Tests.Refresh.Forecast;

using Acme.Workforce.Assets.Refresh.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class RefreshForecastViewChecks
{
    public static void RefreshForecastViewShowsTheStatusInCapitals()
    {
        var item = RefreshForecast.NewRefreshForecast("code-393", "title-741");
        var view = RefreshForecastView.ToRefreshForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RefreshForecastView.ToRefreshForecastViews(new List<RefreshForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RefreshForecastViewShowsTheStatusInCapitals();
    }
}
