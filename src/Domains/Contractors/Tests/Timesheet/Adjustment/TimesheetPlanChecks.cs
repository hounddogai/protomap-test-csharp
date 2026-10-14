namespace Acme.Workforce.Contractors.Tests.Timesheet.Adjustment;

using Acme.Workforce.Contractors.Timesheet.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class TimesheetPlanChecks
{
    public static void TimesheetPlanStartsInProposed()
    {
        var item = TimesheetPlan.NewTimesheetPlan("code-433", "title-301");
        Check.Equal(item.Status, TimesheetPlanStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TimesheetPlanMovesToApproved()
    {
        var item = TimesheetPlan.NewTimesheetPlan("code-433", "title-301");
        try
        {
            item.MoveTo(TimesheetPlanStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TimesheetPlanStatus.Approved, "status");
    }

    public static void TimesheetPlanRejectsMovesFromWithdrawn()
    {
        var item = TimesheetPlan.NewTimesheetPlan("code-433", "title-301");
        item.Status = TimesheetPlanStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(TimesheetPlanStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        TimesheetPlanStartsInProposed();
        TimesheetPlanMovesToApproved();
        TimesheetPlanRejectsMovesFromWithdrawn();
    }
}
