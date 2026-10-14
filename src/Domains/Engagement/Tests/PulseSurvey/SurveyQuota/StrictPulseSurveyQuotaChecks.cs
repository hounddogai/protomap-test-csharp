namespace Acme.Workforce.Engagement.Tests.PulseSurvey.SurveyQuota;

using Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

/// <summary>Checks of survey quota code, run by the project's runner.</summary>
public static class StrictPulseSurveyQuotaChecks
{
    public static void StrictPulseSurveyQuotaStartsInPlanned()
    {
        var item = StrictPulseSurveyQuota.NewStrictPulseSurveyQuota("code-449", "name-167");
        Check.Equal(item.Status, StrictPulseSurveyQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictPulseSurveyQuotaMovesToCompleted()
    {
        var item = StrictPulseSurveyQuota.NewStrictPulseSurveyQuota("code-449", "name-167");
        try
        {
            item.MoveTo(StrictPulseSurveyQuotaStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictPulseSurveyQuotaStatus.Completed, "status");
    }

    public static void StrictPulseSurveyQuotaRejectsMovesFromCancelled()
    {
        var item = StrictPulseSurveyQuota.NewStrictPulseSurveyQuota("code-449", "name-167");
        item.Status = StrictPulseSurveyQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictPulseSurveyQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictPulseSurveyQuotaStartsInPlanned();
        StrictPulseSurveyQuotaMovesToCompleted();
        StrictPulseSurveyQuotaRejectsMovesFromCancelled();
    }
}
