namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class PendingTrainingDueExceptionChecks
{
    public static void PendingTrainingDueExceptionStartsInProposed()
    {
        var item = PendingTrainingDueException.NewPendingTrainingDueException("code-274", "label-497");
        Check.Equal(item.Status, PendingTrainingDueExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingTrainingDueExceptionMovesToRejected()
    {
        var item = PendingTrainingDueException.NewPendingTrainingDueException("code-274", "label-497");
        try
        {
            item.MoveTo(PendingTrainingDueExceptionStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingTrainingDueExceptionStatus.Rejected, "status");
    }

    public static void PendingTrainingDueExceptionRejectsMovesFromWithdrawn()
    {
        var item = PendingTrainingDueException.NewPendingTrainingDueException("code-274", "label-497");
        item.Status = PendingTrainingDueExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(PendingTrainingDueExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        PendingTrainingDueExceptionStartsInProposed();
        PendingTrainingDueExceptionMovesToRejected();
        PendingTrainingDueExceptionRejectsMovesFromWithdrawn();
    }
}
