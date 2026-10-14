namespace Acme.Workforce.Orgdesign.Tests.Headcount.Baseline;

using Acme.Workforce.Orgdesign.Headcount.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class HeadcountForecastViewChecks
{
    public static void HeadcountForecastViewShowsTheStatusInCapitals()
    {
        var item = HeadcountForecast.NewHeadcountForecast("code-171", "name-528");
        var view = HeadcountForecastView.ToHeadcountForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(HeadcountForecastView.ToHeadcountForecastViews(new List<HeadcountForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        HeadcountForecastViewShowsTheStatusInCapitals();
    }
}
