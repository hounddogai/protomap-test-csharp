namespace Acme.Workforce.Compensation.Tests.PayBand.Forecast;

using Acme.Workforce.Compensation.PayBand.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class ScheduledPayBandForecastViewChecks
{
    public static void ScheduledPayBandForecastViewShowsTheStatusInCapitals()
    {
        var item = ScheduledPayBandForecast.NewScheduledPayBandForecast("code-669", "label-428");
        var view = ScheduledPayBandForecastView.ToScheduledPayBandForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledPayBandForecastView.ToScheduledPayBandForecastViews(new List<ScheduledPayBandForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledPayBandForecastViewShowsTheStatusInCapitals();
    }
}
