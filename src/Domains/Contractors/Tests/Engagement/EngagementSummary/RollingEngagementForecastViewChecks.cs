namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class RollingEngagementForecastViewChecks
{
    public static void RollingEngagementForecastViewShowsTheStatusInCapitals()
    {
        var item = RollingEngagementForecast.NewRollingEngagementForecast("code-902", "label-83");
        var view = RollingEngagementForecastView.ToRollingEngagementForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingEngagementForecastView.ToRollingEngagementForecastViews(new List<RollingEngagementForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingEngagementForecastViewShowsTheStatusInCapitals();
    }
}
