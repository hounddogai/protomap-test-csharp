namespace Acme.Workforce.Contractors.Tests.Timesheet.OverrideItem;

using Acme.Workforce.Contractors.Timesheet.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class StrictTimesheetEstimateStoreChecks
{
    public static void MemoryStrictTimesheetEstimateStoreFindsWhatItSaved()
    {
        var store = StrictTimesheetEstimateStore.NewMemoryStrictTimesheetEstimateStore();
        var item = StrictTimesheetEstimate.NewStrictTimesheetEstimate("code-332", "label-232");
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
        MemoryStrictTimesheetEstimateStoreFindsWhatItSaved();
    }
}
