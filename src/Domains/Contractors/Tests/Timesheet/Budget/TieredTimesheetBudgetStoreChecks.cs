namespace Acme.Workforce.Contractors.Tests.Timesheet.Budget;

using Acme.Workforce.Contractors.Timesheet.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class TieredTimesheetBudgetStoreChecks
{
    public static void MemoryTieredTimesheetBudgetStoreFindsWhatItSaved()
    {
        var store = TieredTimesheetBudgetStore.NewMemoryTieredTimesheetBudgetStore();
        var item = TieredTimesheetBudget.NewTieredTimesheetBudget("code-837", "name-811");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryTieredTimesheetBudgetStoreFindsWhatItSaved();
    }
}
