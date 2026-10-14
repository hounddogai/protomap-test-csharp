namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetSegmentChecks
{
    public static void TimesheetSegmentStartsInDraft()
    {
        var item = TimesheetSegment.NewTimesheetSegment("code-494", "name-332");
        Check.Equal(item.Status, TimesheetSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetSegmentMovesToActive()
    {
        var item = TimesheetSegment.NewTimesheetSegment("code-494", "name-332");
        try
        {
            item.MoveTo(TimesheetSegmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetSegmentStatus.Active, "status");
    }

    public static void TimesheetSegmentRejectsMovesFromClosed()
    {
        var item = TimesheetSegment.NewTimesheetSegment("code-494", "name-332");
        item.Status = TimesheetSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(TimesheetSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        TimesheetSegmentStartsInDraft();
        TimesheetSegmentMovesToActive();
        TimesheetSegmentRejectsMovesFromClosed();
    }
}
