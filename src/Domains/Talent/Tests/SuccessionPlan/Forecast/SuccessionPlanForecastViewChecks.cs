namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Forecast;

using Acme.Workforce.Talent.SuccessionPlan.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class SuccessionPlanForecastViewChecks
{
    public static void SuccessionPlanForecastViewShowsTheStatusInCapitals()
    {
        var item = SuccessionPlanForecast.NewSuccessionPlanForecast("code-853", "label-788");
        var view = SuccessionPlanForecastView.ToSuccessionPlanForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SuccessionPlanForecastView.ToSuccessionPlanForecastViews(new List<SuccessionPlanForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SuccessionPlanForecastViewShowsTheStatusInCapitals();
    }
}
