namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class StatementOfWorkWindowChecks
{
    public static void StatementOfWorkWindowStartsInIdle()
    {
        var item = StatementOfWorkWindow.NewStatementOfWorkWindow("code-382", "name-813");
        Check.Equal(item.Status, StatementOfWorkWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkWindowMovesToReady()
    {
        var item = StatementOfWorkWindow.NewStatementOfWorkWindow("code-382", "name-813");
        try
        {
            item.MoveTo(StatementOfWorkWindowStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkWindowStatus.Ready, "status");
    }

    public static void StatementOfWorkWindowRejectsMovesFromStopped()
    {
        var item = StatementOfWorkWindow.NewStatementOfWorkWindow("code-382", "name-813");
        item.Status = StatementOfWorkWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(StatementOfWorkWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkWindowStartsInIdle();
        StatementOfWorkWindowMovesToReady();
        StatementOfWorkWindowRejectsMovesFromStopped();
    }
}
