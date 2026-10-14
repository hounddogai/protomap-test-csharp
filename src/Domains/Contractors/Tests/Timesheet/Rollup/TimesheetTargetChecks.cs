namespace Acme.Workforce.Contractors.Tests.Timesheet.Rollup;

using Acme.Workforce.Contractors.Timesheet.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class TimesheetTargetChecks
{
    public static void TimesheetTargetStartsInCreated()
    {
        var item = TimesheetTarget.NewTimesheetTarget("code-951", "label-864");
        Check.Equal(item.Status, TimesheetTargetStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetTargetMovesToValidated()
    {
        var item = TimesheetTarget.NewTimesheetTarget("code-951", "label-864");
        try
        {
            item.MoveTo(TimesheetTargetStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetTargetStatus.Validated, "status");
    }

    public static void TimesheetTargetRejectsMovesFromRetired()
    {
        var item = TimesheetTarget.NewTimesheetTarget("code-951", "label-864");
        item.Status = TimesheetTargetStatus.Retired;
        Check.Fails(() => item.MoveTo(TimesheetTargetStatus.Created), "move");
    }

    public static void RunAll()
    {
        TimesheetTargetStartsInCreated();
        TimesheetTargetMovesToValidated();
        TimesheetTargetRejectsMovesFromRetired();
    }
}
