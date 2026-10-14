namespace Acme.Workforce.Contractors.Tests.Timesheet.TimesheetEstimate;

using Acme.Workforce.Contractors.Timesheet.TimesheetEstimate;

/// <summary>Checks of timesheet estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetEstimateStoreChecks
{
    public static void MemoryLegacyTimesheetEstimateStoreFindsWhatItSaved()
    {
        var store = LegacyTimesheetEstimateStore.NewMemoryLegacyTimesheetEstimateStore();
        var item = LegacyTimesheetEstimate.NewLegacyTimesheetEstimate("code-889", "title-474");
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
        MemoryLegacyTimesheetEstimateStoreFindsWhatItSaved();
    }
}
