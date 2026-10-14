namespace Acme.Workforce.Contractors.Tests.Rate.Score;

using Acme.Workforce.Contractors.Rate.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class StrictRateForecastViewChecks
{
    public static void StrictRateForecastViewShowsTheStatusInCapitals()
    {
        var item = StrictRateForecast.NewStrictRateForecast("code-924", "title-332");
        var view = StrictRateForecastView.ToStrictRateForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(StrictRateForecastView.ToStrictRateForecastViews(new List<StrictRateForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        StrictRateForecastViewShowsTheStatusInCapitals();
    }
}
