namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class FallbackStatementOfWorkScheduleChecks
{
    public static void FallbackStatementOfWorkScheduleStartsInPlanned()
    {
        var item = FallbackStatementOfWorkSchedule.NewFallbackStatementOfWorkSchedule("code-431", "title-623");
        Check.Equal(item.Status, FallbackStatementOfWorkScheduleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackStatementOfWorkScheduleMovesToScheduled()
    {
        var item = FallbackStatementOfWorkSchedule.NewFallbackStatementOfWorkSchedule("code-431", "title-623");
        try
        {
            item.MoveTo(FallbackStatementOfWorkScheduleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackStatementOfWorkScheduleStatus.Scheduled, "status");
    }

    public static void FallbackStatementOfWorkScheduleRejectsMovesFromCancelled()
    {
        var item = FallbackStatementOfWorkSchedule.NewFallbackStatementOfWorkSchedule("code-431", "title-623");
        item.Status = FallbackStatementOfWorkScheduleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackStatementOfWorkScheduleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackStatementOfWorkScheduleStartsInPlanned();
        FallbackStatementOfWorkScheduleMovesToScheduled();
        FallbackStatementOfWorkScheduleRejectsMovesFromCancelled();
    }
}
