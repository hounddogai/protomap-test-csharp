namespace Acme.Workforce.Contractors.Tests.Timesheet.Exception;

using Acme.Workforce.Contractors.Timesheet.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TimesheetAssignmentChecks
{
    public static void TimesheetAssignmentStartsInDraft()
    {
        var item = TimesheetAssignment.NewTimesheetAssignment("code-408", "title-526");
        Check.Equal(item.Status, TimesheetAssignmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetAssignmentMovesToActive()
    {
        var item = TimesheetAssignment.NewTimesheetAssignment("code-408", "title-526");
        try
        {
            item.MoveTo(TimesheetAssignmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetAssignmentStatus.Active, "status");
    }

    public static void TimesheetAssignmentRejectsMovesFromClosed()
    {
        var item = TimesheetAssignment.NewTimesheetAssignment("code-408", "title-526");
        item.Status = TimesheetAssignmentStatus.Closed;
        Check.Fails(() => item.MoveTo(TimesheetAssignmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TimesheetAssignmentStartsInDraft();
        TimesheetAssignmentMovesToActive();
        TimesheetAssignmentRejectsMovesFromClosed();
    }
}
