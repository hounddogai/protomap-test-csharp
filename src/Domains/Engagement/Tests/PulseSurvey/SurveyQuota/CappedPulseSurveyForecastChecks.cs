namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class CappedPulseSurveyForecastChecks
{
    public static void CappedPulseSurveyForecastStartsInProposed()
    {
        var item = CappedPulseSurveyForecast.NewCappedPulseSurveyForecast("code-350", "title-456");
        Check.Equal(item.Status, CappedPulseSurveyForecastStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedPulseSurveyForecastMovesToApproved()
    {
        var item = CappedPulseSurveyForecast.NewCappedPulseSurveyForecast("code-350", "title-456");
        try
        {
            item.MoveTo(CappedPulseSurveyForecastStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedPulseSurveyForecastStatus.Approved, "status");
    }

    public static void CappedPulseSurveyForecastRejectsMovesFromWithdrawn()
    {
        var item = CappedPulseSurveyForecast.NewCappedPulseSurveyForecast("code-350", "title-456");
        item.Status = CappedPulseSurveyForecastStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(CappedPulseSurveyForecastStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        CappedPulseSurveyForecastStartsInProposed();
        CappedPulseSurveyForecastMovesToApproved();
        CappedPulseSurveyForecastRejectsMovesFromWithdrawn();
    }
}
