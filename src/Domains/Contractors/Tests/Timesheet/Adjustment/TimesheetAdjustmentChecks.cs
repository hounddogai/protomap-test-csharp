namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetAdjustmentChecks
{
    public static void TimesheetAdjustmentStartsInRequested()
    {
        var item = TimesheetAdjustment.NewTimesheetAdjustment("code-820", "title-220");
        Check.Equal(item.Status, TimesheetAdjustmentStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetAdjustmentMovesToRevoked()
    {
        var item = TimesheetAdjustment.NewTimesheetAdjustment("code-820", "title-220");
        try
        {
            item.MoveTo(TimesheetAdjustmentStatus.Revoked);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetAdjustmentStatus.Revoked, "status");
    }

    public static void TimesheetAdjustmentRejectsMovesFromExpired()
    {
        var item = TimesheetAdjustment.NewTimesheetAdjustment("code-820", "title-220");
        item.Status = TimesheetAdjustmentStatus.Expired;
        Check.Fails(() => item.MoveTo(TimesheetAdjustmentStatus.Requested), "move");
    }

    public static void RunAll()
    {
        TimesheetAdjustmentStartsInRequested();
        TimesheetAdjustmentMovesToRevoked();
        TimesheetAdjustmentRejectsMovesFromExpired();
    }
}
