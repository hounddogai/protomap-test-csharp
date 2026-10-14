namespace Acme.Workforce.Contractors.Tests.Timesheet.Audit;

using Acme.Workforce.Contractors.Timesheet.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class PendingTimesheetSummaryStoreChecks
{
    public static void MemoryPendingTimesheetSummaryStoreFindsWhatItSaved()
    {
        var store = PendingTimesheetSummaryStore.NewMemoryPendingTimesheetSummaryStore();
        var item = PendingTimesheetSummary.NewPendingTimesheetSummary("code-943", "title-847");
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
        MemoryPendingTimesheetSummaryStoreFindsWhatItSaved();
    }
}
