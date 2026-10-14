namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyForecastViewChecks
{
    public static void PulseSurveyForecastViewShowsTheStatusInCapitals()
    {
        var item = PulseSurveyForecast.NewPulseSurveyForecast("code-769", "title-999");
        var view = PulseSurveyForecastView.ToPulseSurveyForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PulseSurveyForecastView.ToPulseSurveyForecastViews(new List<PulseSurveyForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PulseSurveyForecastViewShowsTheStatusInCapitals();
    }
}
