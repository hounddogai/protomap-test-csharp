namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Target;

using Acme.Workforce.Engagement.PulseSurvey.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class PendingPulseSurveyAuditChecks
{
    public static void PendingPulseSurveyAuditStartsInIdle()
    {
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("code-963", "title-963");
        Check.Equal(item.Status, PendingPulseSurveyAuditStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingPulseSurveyAuditMovesToWarming()
    {
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("code-963", "title-963");
        try
        {
            item.MoveTo(PendingPulseSurveyAuditStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingPulseSurveyAuditStatus.Warming, "status");
    }

    public static void PendingPulseSurveyAuditRejectsMovesFromStopped()
    {
        var item = PendingPulseSurveyAudit.NewPendingPulseSurveyAudit("code-963", "title-963");
        item.Status = PendingPulseSurveyAuditStatus.Stopped;
        Check.Fails(() => item.MoveTo(PendingPulseSurveyAuditStatus.Idle), "move");
    }

    public static void RunAll()
    {
        PendingPulseSurveyAuditStartsInIdle();
        PendingPulseSurveyAuditMovesToWarming();
        PendingPulseSurveyAuditRejectsMovesFromStopped();
    }
}
