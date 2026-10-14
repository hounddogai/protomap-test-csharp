namespace Acme.Workforce.Contractors.Tests.Timesheet.RecordItem;

using Acme.Workforce.Contractors.Timesheet.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class TimesheetMilestoneChecks
{
    public static void TimesheetMilestoneStartsInProposed()
    {
        var item = TimesheetMilestone.NewTimesheetMilestone("code-59", "name-458");
        Check.Equal(item.Status, TimesheetMilestoneStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetMilestoneMovesToWithdrawn()
    {
        var item = TimesheetMilestone.NewTimesheetMilestone("code-59", "name-458");
        try
        {
            item.MoveTo(TimesheetMilestoneStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetMilestoneStatus.Withdrawn, "status");
    }

    public static void TimesheetMilestoneRejectsMovesFromWithdrawn()
    {
        var item = TimesheetMilestone.NewTimesheetMilestone("code-59", "name-458");
        item.Status = TimesheetMilestoneStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TimesheetMilestoneStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TimesheetMilestoneStartsInProposed();
        TimesheetMilestoneMovesToWithdrawn();
        TimesheetMilestoneRejectsMovesFromWithdrawn();
    }
}
