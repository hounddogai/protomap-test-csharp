namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingTrainingDueExceptionChecks
{
    public static void RollingTrainingDueExceptionStartsInProposed()
    {
        var item = RollingTrainingDueException.NewRollingTrainingDueException("code-273", "name-751");
        Check.Equal(item.Status, RollingTrainingDueExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingTrainingDueExceptionMovesToApproved()
    {
        var item = RollingTrainingDueException.NewRollingTrainingDueException("code-273", "name-751");
        try
        {
            item.MoveTo(RollingTrainingDueExceptionStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingTrainingDueExceptionStatus.Approved, "status");
    }

    public static void RollingTrainingDueExceptionRejectsMovesFromWithdrawn()
    {
        var item = RollingTrainingDueException.NewRollingTrainingDueException("code-273", "name-751");
        item.Status = RollingTrainingDueExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(RollingTrainingDueExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        RollingTrainingDueExceptionStartsInProposed();
        RollingTrainingDueExceptionMovesToApproved();
        RollingTrainingDueExceptionRejectsMovesFromWithdrawn();
    }
}
