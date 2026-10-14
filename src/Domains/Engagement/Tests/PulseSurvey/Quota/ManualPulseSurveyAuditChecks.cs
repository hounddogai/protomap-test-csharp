namespace Acme.Workforce.Engagement.Tests.PulseSurvey.Quota;

using Acme.Workforce.Engagement.PulseSurvey.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class ManualPulseSurveyAuditChecks
{
    public static void ManualPulseSurveyAuditStartsInProposed()
    {
        var item = ManualPulseSurveyAudit.NewManualPulseSurveyAudit("code-444", "label-295");
        Check.Equal(item.Status, ManualPulseSurveyAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualPulseSurveyAuditMovesToApproved()
    {
        var item = ManualPulseSurveyAudit.NewManualPulseSurveyAudit("code-444", "label-295");
        try
        {
            item.MoveTo(ManualPulseSurveyAuditStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualPulseSurveyAuditStatus.Approved, "status");
    }

    public static void ManualPulseSurveyAuditRejectsMovesFromWithdrawn()
    {
        var item = ManualPulseSurveyAudit.NewManualPulseSurveyAudit("code-444", "label-295");
        item.Status = ManualPulseSurveyAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ManualPulseSurveyAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ManualPulseSurveyAuditStartsInProposed();
        ManualPulseSurveyAuditMovesToApproved();
        ManualPulseSurveyAuditRejectsMovesFromWithdrawn();
    }
}
