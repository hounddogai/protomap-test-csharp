namespace Acme.Workforce.Contractors.Tests.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TieredTimesheetBudgetViewChecks
{
    public static void TieredTimesheetBudgetViewShowsTheStatusInCapitals()
    {
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("code-837", "name-811");
        var view = TieredTimesheetBudgetView.ToTieredTimesheetBudgetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredTimesheetBudgetView.ToTieredTimesheetBudgetViews(new List<TieredTimesheetBudget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredTimesheetBudgetViewShowsTheStatusInCapitals();
    }
}
