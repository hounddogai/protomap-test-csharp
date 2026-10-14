namespace Acme.Workforce.Contractors.Tests.Timesheet.Tier;

using Acme.Workforce.Contractors.Timesheet.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class TimesheetAllocationChecks
{
    public static void TimesheetAllocationStartsInIdle()
    {
        var item = TimesheetAllocation.NewTimesheetAllocation("code-437", "label-358");
        Check.Equal(item.Status, TimesheetAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetAllocationMovesToWarming()
    {
        var item = TimesheetAllocation.NewTimesheetAllocation("code-437", "label-358");
        try
        {
            item.MoveTo(TimesheetAllocationStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetAllocationStatus.Warming, "status");
    }

    public static void TimesheetAllocationRejectsMovesFromStopped()
    {
        var item = TimesheetAllocation.NewTimesheetAllocation("code-437", "label-358");
        item.Status = TimesheetAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(TimesheetAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TimesheetAllocationStartsInIdle();
        TimesheetAllocationMovesToWarming();
        TimesheetAllocationRejectsMovesFromStopped();
    }
}
