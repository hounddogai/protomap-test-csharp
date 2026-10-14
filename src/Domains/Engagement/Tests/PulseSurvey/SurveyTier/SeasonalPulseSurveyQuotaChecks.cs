namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyTier;

using Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

/// <summary>Checks of survey tier code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyQuotaChecks
{
    public static void SeasonalPulseSurveyQuotaStartsInProposed()
    {
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("code-535", "label-234");
        Check.Equal(item.Status, SeasonalPulseSurveyQuotaStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPulseSurveyQuotaMovesToRejected()
    {
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("code-535", "label-234");
        try
        {
            item.MoveTo(SeasonalPulseSurveyQuotaStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPulseSurveyQuotaStatus.Rejected, "status");
    }

    public static void SeasonalPulseSurveyQuotaRejectsMovesFromWithdrawn()
    {
        var item = SeasonalPulseSurveyQuota.NewSeasonalPulseSurveyQuota("code-535", "label-234");
        item.Status = SeasonalPulseSurveyQuotaStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalPulseSurveyQuotaStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyQuotaStartsInProposed();
        SeasonalPulseSurveyQuotaMovesToRejected();
        SeasonalPulseSurveyQuotaRejectsMovesFromWithdrawn();
    }
}
