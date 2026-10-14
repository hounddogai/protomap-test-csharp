namespace Acme.Workforce.Contractors.Tests.Timesheet.Group;

using Acme.Workforce.Contractors.Timesheet.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ScheduledTimesheetGroupChecks
{
    public static void ScheduledTimesheetGroupStartsInProposed()
    {
        var item = ScheduledTimesheetGroup.NewScheduledTimesheetGroup("code-664", "label-877");
        Check.Equal(item.Status, ScheduledTimesheetGroupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledTimesheetGroupMovesToApproved()
    {
        var item = ScheduledTimesheetGroup.NewScheduledTimesheetGroup("code-664", "label-877");
        try
        {
            item.MoveTo(ScheduledTimesheetGroupStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledTimesheetGroupStatus.Approved, "status");
    }

    public static void ScheduledTimesheetGroupRejectsMovesFromWithdrawn()
    {
        var item = ScheduledTimesheetGroup.NewScheduledTimesheetGroup("code-664", "label-877");
        item.Status = ScheduledTimesheetGroupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(ScheduledTimesheetGroupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        ScheduledTimesheetGroupStartsInProposed();
        ScheduledTimesheetGroupMovesToApproved();
        ScheduledTimesheetGroupRejectsMovesFromWithdrawn();
    }
}
