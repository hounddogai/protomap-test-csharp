namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Assignment;

using Acme.Workforce.Contractors.StatementOfWork.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class StatementOfWorkScheduleChecks
{
    public static void StatementOfWorkScheduleStartsInDraft()
    {
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("code-569", "name-787");
        Check.Equal(item.Status, StatementOfWorkScheduleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StatementOfWorkScheduleMovesToClosed()
    {
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("code-569", "name-787");
        try
        {
            item.MoveTo(StatementOfWorkScheduleStatus.Closed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StatementOfWorkScheduleStatus.Closed, "status");
    }

    public static void StatementOfWorkScheduleRejectsMovesFromClosed()
    {
        var item = StatementOfWorkSchedule.NewStatementOfWorkSchedule("code-569", "name-787");
        item.Status = StatementOfWorkScheduleStatus.Closed;
        Check.Fails(() => item.MoveTo(StatementOfWorkScheduleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        StatementOfWorkScheduleStartsInDraft();
        StatementOfWorkScheduleMovesToClosed();
        StatementOfWorkScheduleRejectsMovesFromClosed();
    }
}
