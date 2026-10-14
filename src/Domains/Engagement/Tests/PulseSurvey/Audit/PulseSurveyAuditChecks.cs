namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Audit;

using Acme.Workforce.Engagement.PulseSurvey.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PulseSurveyAuditChecks
{
    public static void PulseSurveyAuditStartsInDraft()
    {
        var item = PulseSurveyAudit.NewPulseSurveyAudit("code-250", "label-477");
        Check.Equal(item.Status, PulseSurveyAuditStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PulseSurveyAuditMovesToPending()
    {
        var item = PulseSurveyAudit.NewPulseSurveyAudit("code-250", "label-477");
        try
        {
            item.MoveTo(PulseSurveyAuditStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PulseSurveyAuditStatus.Pending, "status");
    }

    public static void PulseSurveyAuditRejectsMovesFromClosed()
    {
        var item = PulseSurveyAudit.NewPulseSurveyAudit("code-250", "label-477");
        item.Status = PulseSurveyAuditStatus.Closed;
        Check.Fails(() => item.MoveTo(PulseSurveyAuditStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PulseSurveyAuditStartsInDraft();
        PulseSurveyAuditMovesToPending();
        PulseSurveyAuditRejectsMovesFromClosed();
    }
}
