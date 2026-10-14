namespace Acme.Workforce.Compliance.Tests.TrainingDue.Mapping;

using Acme.Workforce.Compliance.TrainingDue.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class DefaultTrainingDueAuditChecks
{
    public static void DefaultTrainingDueAuditStartsInProposed()
    {
        var item = DefaultTrainingDueAudit.NewDefaultTrainingDueAudit("code-558", "title-74");
        Check.Equal(item.Status, DefaultTrainingDueAuditStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultTrainingDueAuditMovesToRejected()
    {
        var item = DefaultTrainingDueAudit.NewDefaultTrainingDueAudit("code-558", "title-74");
        try
        {
            item.MoveTo(DefaultTrainingDueAuditStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultTrainingDueAuditStatus.Rejected, "status");
    }

    public static void DefaultTrainingDueAuditRejectsMovesFromWithdrawn()
    {
        var item = DefaultTrainingDueAudit.NewDefaultTrainingDueAudit("code-558", "title-74");
        item.Status = DefaultTrainingDueAuditStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(DefaultTrainingDueAuditStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        DefaultTrainingDueAuditStartsInProposed();
        DefaultTrainingDueAuditMovesToRejected();
        DefaultTrainingDueAuditRejectsMovesFromWithdrawn();
    }
}
