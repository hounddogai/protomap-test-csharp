namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Estimate;

using Acme.Workforce.Engagement.PulseSurvey.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PulseSurveyForecastChecks
{
    public static void PulseSurveyForecastStartsInDraft()
    {
        var item = PulseSurveyForecast.NewPulseSurveyForecast("code-769", "title-999");
        Check.Equal(item.Status, PulseSurveyForecastStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyForecastMovesToActive()
    {
        var item = PulseSurveyForecast.NewPulseSurveyForecast("code-769", "title-999");
        try
        {
            item.MoveTo(PulseSurveyForecastStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyForecastStatus.Active, "status");
    }

    public static void PulseSurveyForecastRejectsMovesFromClosed()
    {
        var item = PulseSurveyForecast.NewPulseSurveyForecast("code-769", "title-999");
        item.Status = PulseSurveyForecastStatus.Closed;
        Check.Fails(() => item.MoveTo(PulseSurveyForecastStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PulseSurveyForecastStartsInDraft();
        PulseSurveyForecastMovesToActive();
        PulseSurveyForecastRejectsMovesFromClosed();
    }
}
