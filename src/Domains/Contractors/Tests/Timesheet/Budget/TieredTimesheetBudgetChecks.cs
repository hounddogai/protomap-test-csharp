namespace Acme.Workforce.Contractors.Tests.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TieredTimesheetBudgetChecks
{
    public static void TieredTimesheetBudgetStartsInIdle()
    {
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("code-837", "name-811");
        Check.Equal(item.Status, TieredTimesheetBudgetStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredTimesheetBudgetMovesToDraining()
    {
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("code-837", "name-811");
        try
        {
            item.MoveTo(TieredTimesheetBudgetStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredTimesheetBudgetStatus.Draining, "status");
    }

    public static void TieredTimesheetBudgetRejectsMovesFromStopped()
    {
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("code-837", "name-811");
        item.Status = TieredTimesheetBudgetStatus.Stopped;
        Check.Fails(() => item.MoveTo(TieredTimesheetBudgetStatus.Idle), "move");
    }

    public static void RunAll()
    {
        TieredTimesheetBudgetStartsInIdle();
        TieredTimesheetBudgetMovesToDraining();
        TieredTimesheetBudgetRejectsMovesFromStopped();
    }
}
