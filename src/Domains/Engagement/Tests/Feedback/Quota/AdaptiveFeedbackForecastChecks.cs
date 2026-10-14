namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AdaptiveFeedbackForecastChecks
{
    public static void AdaptiveFeedbackForecastStartsInNew()
    {
        var item = AdaptiveFeedbackForecast.NewAdaptiveFeedbackForecast("code-458", "title-390");
        Check.Equal(item.Status, AdaptiveFeedbackForecastStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdaptiveFeedbackForecastMovesToQueued()
    {
        var item = AdaptiveFeedbackForecast.NewAdaptiveFeedbackForecast("code-458", "title-390");
        try
        {
            item.MoveTo(AdaptiveFeedbackForecastStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdaptiveFeedbackForecastStatus.Queued, "status");
    }

    public static void AdaptiveFeedbackForecastRejectsMovesFromFailed()
    {
        var item = AdaptiveFeedbackForecast.NewAdaptiveFeedbackForecast("code-458", "title-390");
        item.Status = AdaptiveFeedbackForecastStatus.Failed;
        Check.Fails(() => item.MoveTo(AdaptiveFeedbackForecastStatus.New), "move");
    }

    public static void RunAll()
    {
        AdaptiveFeedbackForecastStartsInNew();
        AdaptiveFeedbackForecastMovesToQueued();
        AdaptiveFeedbackForecastRejectsMovesFromFailed();
    }
}
