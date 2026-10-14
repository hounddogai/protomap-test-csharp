namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualPulseSurveyForecastViewChecks
{
    public static void ManualPulseSurveyForecastViewShowsTheStatusInCapitals()
    {
        var item = ManualPulseSurveyForecast.NewManualPulseSurveyForecast("code-937", "label-172");
        var view = ManualPulseSurveyForecastView.ToManualPulseSurveyForecastView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualPulseSurveyForecastView.ToManualPulseSurveyForecastViews(new List<ManualPulseSurveyForecast> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualPulseSurveyForecastViewShowsTheStatusInCapitals();
    }
}
