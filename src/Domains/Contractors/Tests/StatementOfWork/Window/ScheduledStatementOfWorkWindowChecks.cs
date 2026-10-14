namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Window;

using Acme.Workforce.Contractors.StatementOfWork.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class ScheduledStatementOfWorkWindowChecks
{
    public static void ScheduledStatementOfWorkWindowStartsInIdle()
    {
        var item = ScheduledStatementOfWorkWindow.NewScheduledStatementOfWorkWindow("code-344", "label-241");
        Check.Equal(item.Status, ScheduledStatementOfWorkWindowStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledStatementOfWorkWindowMovesToDraining()
    {
        var item = ScheduledStatementOfWorkWindow.NewScheduledStatementOfWorkWindow("code-344", "label-241");
        try
        {
            item.MoveTo(ScheduledStatementOfWorkWindowStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledStatementOfWorkWindowStatus.Draining, "status");
    }

    public static void ScheduledStatementOfWorkWindowRejectsMovesFromStopped()
    {
        var item = ScheduledStatementOfWorkWindow.NewScheduledStatementOfWorkWindow("code-344", "label-241");
        item.Status = ScheduledStatementOfWorkWindowStatus.Stopped;
        Check.Fails(() => item.MoveTo(ScheduledStatementOfWorkWindowStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ScheduledStatementOfWorkWindowStartsInIdle();
        ScheduledStatementOfWorkWindowMovesToDraining();
        ScheduledStatementOfWorkWindowRejectsMovesFromStopped();
    }
}
