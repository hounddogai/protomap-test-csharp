namespace Acme.Workforce.Contractors.Tests.StatementOfWork.Schedule;

using Acme.Workforce.Contractors.StatementOfWork.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class StrictStatementOfWorkAssignmentChecks
{
    public static void StrictStatementOfWorkAssignmentStartsInPlanned()
    {
        var item = StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment("code-49", "name-369");
        Check.Equal(item.Status, StrictStatementOfWorkAssignmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictStatementOfWorkAssignmentMovesToScheduled()
    {
        var item = StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment("code-49", "name-369");
        try
        {
            item.MoveTo(StrictStatementOfWorkAssignmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictStatementOfWorkAssignmentStatus.Scheduled, "status");
    }

    public static void StrictStatementOfWorkAssignmentRejectsMovesFromCancelled()
    {
        var item = StrictStatementOfWorkAssignment.NewStrictStatementOfWorkAssignment("code-49", "name-369");
        item.Status = StrictStatementOfWorkAssignmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(StrictStatementOfWorkAssignmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        StrictStatementOfWorkAssignmentStartsInPlanned();
        StrictStatementOfWorkAssignmentMovesToScheduled();
        StrictStatementOfWorkAssignmentRejectsMovesFromCancelled();
    }
}
