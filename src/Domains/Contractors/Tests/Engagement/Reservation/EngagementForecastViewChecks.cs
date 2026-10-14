namespace Acme.Workforce.Contractors.Tests.Engagement.Reservation;

using Acme.Workforce.Contractors.Engagement.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class EngagementForecastViewChecks
{
    public static void EngagementForecastViewShowsTheStatusInCapitals()
    {
        var item = EngagementForecast.NewEngagementForecast("code-775", "label-414");
        var view = EngagementForecastView.ToEngagementForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(EngagementForecastView.ToEngagementForecastViews(new List<EngagementForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        EngagementForecastViewShowsTheStatusInCapitals();
    }
}
