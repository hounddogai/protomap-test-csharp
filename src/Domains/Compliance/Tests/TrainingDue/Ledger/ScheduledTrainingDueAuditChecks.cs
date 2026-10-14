namespace Acme.Workforce.Compliance.Tests.TrainingDue.Ledger;

using Acme.Workforce.Compliance.TrainingDue.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class ScheduledTrainingDueAuditChecks
{
    public static void ScheduledTrainingDueAuditStartsInCreated()
    {
        var item = ScheduledTrainingDueAudit.NewScheduledTrainingDueAudit("code-827", "name-935");
        Check.Equal(item.Status, ScheduledTrainingDueAuditStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledTrainingDueAuditMovesToPublished()
    {
        var item = ScheduledTrainingDueAudit.NewScheduledTrainingDueAudit("code-827", "name-935");
        try
        {
            item.MoveTo(ScheduledTrainingDueAuditStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledTrainingDueAuditStatus.Published, "status");
    }

    public static void ScheduledTrainingDueAuditRejectsMovesFromRetired()
    {
        var item = ScheduledTrainingDueAudit.NewScheduledTrainingDueAudit("code-827", "name-935");
        item.Status = ScheduledTrainingDueAuditStatus.Retired;
        Check.Fails(() => item.MoveTo(ScheduledTrainingDueAuditStatus.Created), "move");
    }

    public static void RunAll()
    {
        ScheduledTrainingDueAuditStartsInCreated();
        ScheduledTrainingDueAuditMovesToPublished();
        ScheduledTrainingDueAuditRejectsMovesFromRetired();
    }
}
