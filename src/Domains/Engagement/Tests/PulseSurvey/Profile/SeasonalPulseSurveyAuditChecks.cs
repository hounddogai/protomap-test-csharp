namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Profile;

using Acme.Workforce.Engagement.PulseSurvey.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalPulseSurveyAuditChecks
{
    public static void SeasonalPulseSurveyAuditStartsInRequested()
    {
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("code-436", "label-679");
        Check.Equal(item.Status, SeasonalPulseSurveyAuditStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPulseSurveyAuditMovesToRevoked()
    {
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("code-436", "label-679");
        try
        {
            item.MoveTo(SeasonalPulseSurveyAuditStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPulseSurveyAuditStatus.Revoked, "status");
    }

    public static void SeasonalPulseSurveyAuditRejectsMovesFromExpired()
    {
        var item = SeasonalPulseSurveyAudit.NewSeasonalPulseSurveyAudit("code-436", "label-679");
        item.Status = SeasonalPulseSurveyAuditStatus.Expired;
        Check.Fails(() => item.MoveTo(SeasonalPulseSurveyAuditStatus.Requested), "move");
    }

    public static void RunAll()
    {
        SeasonalPulseSurveyAuditStartsInRequested();
        SeasonalPulseSurveyAuditMovesToRevoked();
        SeasonalPulseSurveyAuditRejectsMovesFromExpired();
    }
}
