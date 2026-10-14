namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TrainingDueAuditChecks
{
    public static void TrainingDueAuditStartsInPlanned()
    {
        var item = TrainingDueAudit.NewTrainingDueAudit("code-898", "title-406");
        Check.Equal(item.Status, TrainingDueAuditStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TrainingDueAuditMovesToCancelled()
    {
        var item = TrainingDueAudit.NewTrainingDueAudit("code-898", "title-406");
        try
        {
            item.MoveTo(TrainingDueAuditStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TrainingDueAuditStatus.Cancelled, "status");
    }

    public static void TrainingDueAuditRejectsMovesFromCancelled()
    {
        var item = TrainingDueAudit.NewTrainingDueAudit("code-898", "title-406");
        item.Status = TrainingDueAuditStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TrainingDueAuditStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TrainingDueAuditStartsInPlanned();
        TrainingDueAuditMovesToCancelled();
        TrainingDueAuditRejectsMovesFromCancelled();
    }
}
