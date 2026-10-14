namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ManualPulseSurveyQuotaChecks
{
    public static void ManualPulseSurveyQuotaStartsInPlanned()
    {
        var item = ManualPulseSurveyQuota.NewManualPulseSurveyQuota("code-670", "name-524");
        Check.Equal(item.Status, ManualPulseSurveyQuotaStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPulseSurveyQuotaMovesToScheduled()
    {
        var item = ManualPulseSurveyQuota.NewManualPulseSurveyQuota("code-670", "name-524");
        try
        {
            item.MoveTo(ManualPulseSurveyQuotaStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPulseSurveyQuotaStatus.Scheduled, "status");
    }

    public static void ManualPulseSurveyQuotaRejectsMovesFromCancelled()
    {
        var item = ManualPulseSurveyQuota.NewManualPulseSurveyQuota("code-670", "name-524");
        item.Status = ManualPulseSurveyQuotaStatus.Cancelled;
        Check.Fails(() => item.MoveTo(ManualPulseSurveyQuotaStatus.Planned), "move");
    }

    public static void RunAll()
    {
        ManualPulseSurveyQuotaStartsInPlanned();
        ManualPulseSurveyQuotaMovesToScheduled();
        ManualPulseSurveyQuotaRejectsMovesFromCancelled();
    }
}
