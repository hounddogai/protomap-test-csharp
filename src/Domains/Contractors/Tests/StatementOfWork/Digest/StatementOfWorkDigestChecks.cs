namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Digest;

using Acme.Workforce.Contractors.StatementOfWork.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class StatementOfWorkDigestChecks
{
    public static void StatementOfWorkDigestStartsInIdle()
    {
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("code-891", "name-450");
        Check.Equal(item.Status, StatementOfWorkDigestStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkDigestMovesToStopped()
    {
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("code-891", "name-450");
        try
        {
            item.MoveTo(StatementOfWorkDigestStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkDigestStatus.Stopped, "status");
    }

    public static void StatementOfWorkDigestRejectsMovesFromStopped()
    {
        var item = StatementOfWorkDigest.NewStatementOfWorkDigest("code-891", "name-450");
        item.Status = StatementOfWorkDigestStatus.Stopped;
        Check.Fails(() => item.MoveTo(StatementOfWorkDigestStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkDigestStartsInIdle();
        StatementOfWorkDigestMovesToStopped();
        StatementOfWorkDigestRejectsMovesFromStopped();
    }
}
