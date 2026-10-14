namespace Acme.Workforce.Contractors.Tests.Timesheet.Estimate;

using Acme.Workforce.Contractors.Timesheet.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class LegacyTimesheetSummaryStoreChecks
{
    public static void MemoryLegacyTimesheetSummaryStoreFindsWhatItSaved()
    {
        var store = LegacyTimesheetSummaryStore.NewMemoryLegacyTimesheetSummaryStore();
        var item = LegacyTimesheetSummary.NewLegacyTimesheetSummary("code-294", "label-1");
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
        MemoryLegacyTimesheetSummaryStoreFindsWhatItSaved();
    }
}
