namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Snapshot;

using Acme.Workforce.Contractors.StatementOfWork.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class StatementOfWorkExceptionChecks
{
    public static void StatementOfWorkExceptionStartsInProposed()
    {
        var item = StatementOfWorkException.NewStatementOfWorkException("code-34", "name-98");
        Check.Equal(item.Status, StatementOfWorkExceptionStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkExceptionMovesToRejected()
    {
        var item = StatementOfWorkException.NewStatementOfWorkException("code-34", "name-98");
        try
        {
            item.MoveTo(StatementOfWorkExceptionStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkExceptionStatus.Rejected, "status");
    }

    public static void StatementOfWorkExceptionRejectsMovesFromWithdrawn()
    {
        var item = StatementOfWorkException.NewStatementOfWorkException("code-34", "name-98");
        item.Status = StatementOfWorkExceptionStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(StatementOfWorkExceptionStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkExceptionStartsInProposed();
        StatementOfWorkExceptionMovesToRejected();
        StatementOfWorkExceptionRejectsMovesFromWithdrawn();
    }
}
