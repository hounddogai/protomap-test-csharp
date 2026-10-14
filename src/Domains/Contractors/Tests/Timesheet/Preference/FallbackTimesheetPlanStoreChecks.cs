namespace Acme.Workforce.Contractors.Tests.Timesheet.Preference;

using Acme.Workforce.Contractors.Timesheet.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FallbackTimesheetPlanStoreChecks
{
    public static void MemoryFallbackTimesheetPlanStoreFindsWhatItSaved()
    {
        var store = FallbackTimesheetPlanStore.NewMemoryFallbackTimesheetPlanStore();
        var item = FallbackTimesheetPlan.NewFallbackTimesheetPlan("code-925", "name-646");
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
        MemoryFallbackTimesheetPlanStoreFindsWhatItSaved();
    }
}
