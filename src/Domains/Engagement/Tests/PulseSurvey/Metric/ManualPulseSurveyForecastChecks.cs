namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Metric;

using Acme.Workforce.Engagement.PulseSurvey.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualPulseSurveyForecastChecks
{
    public static void ManualPulseSurveyForecastStartsInIdle()
    {
        var item = ManualPulseSurveyForecast.NewManualPulseSurveyForecast("code-937", "label-172");
        Check.Equal(item.Status, ManualPulseSurveyForecastStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPulseSurveyForecastMovesToWarming()
    {
        var item = ManualPulseSurveyForecast.NewManualPulseSurveyForecast("code-937", "label-172");
        try
        {
            item.MoveTo(ManualPulseSurveyForecastStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPulseSurveyForecastStatus.Warming, "status");
    }

    public static void ManualPulseSurveyForecastRejectsMovesFromStopped()
    {
        var item = ManualPulseSurveyForecast.NewManualPulseSurveyForecast("code-937", "label-172");
        item.Status = ManualPulseSurveyForecastStatus.Stopped;
        Check.Fails(() => item.MoveTo(ManualPulseSurveyForecastStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ManualPulseSurveyForecastStartsInIdle();
        ManualPulseSurveyForecastMovesToWarming();
        ManualPulseSurveyForecastRejectsMovesFromStopped();
    }
}
