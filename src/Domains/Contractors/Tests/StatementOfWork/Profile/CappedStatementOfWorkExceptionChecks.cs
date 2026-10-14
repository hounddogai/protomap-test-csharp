namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Profile;

using Acme.Workforce.Contractors.StatementOfWork.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class CappedStatementOfWorkExceptionChecks
{
    public static void CappedStatementOfWorkExceptionStartsInIdle()
    {
        var item = CappedStatementOfWorkException.NewCappedStatementOfWorkException("code-789", "name-124");
        Check.Equal(item.Status, CappedStatementOfWorkExceptionStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void CappedStatementOfWorkExceptionMovesToReady()
    {
        var item = CappedStatementOfWorkException.NewCappedStatementOfWorkException("code-789", "name-124");
        try
        {
            item.MoveTo(CappedStatementOfWorkExceptionStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, CappedStatementOfWorkExceptionStatus.Ready, "status");
    }

    public static void CappedStatementOfWorkExceptionRejectsMovesFromStopped()
    {
        var item = CappedStatementOfWorkException.NewCappedStatementOfWorkException("code-789", "name-124");
        item.Status = CappedStatementOfWorkExceptionStatus.Stopped;
        Check.Fails(() => item.MoveTo(CappedStatementOfWorkExceptionStatus.Idle), "move");
    }

    public static void RunAll()
    {
        CappedStatementOfWorkExceptionStartsInIdle();
        CappedStatementOfWorkExceptionMovesToReady();
        CappedStatementOfWorkExceptionRejectsMovesFromStopped();
    }
}
