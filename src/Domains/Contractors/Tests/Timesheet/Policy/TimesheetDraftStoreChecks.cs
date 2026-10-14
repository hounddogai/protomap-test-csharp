namespace Acme.Workforce.Contractors.Tests.Timesheet.Policy;

using Acme.Workforce.Contractors.Timesheet.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class TimesheetDraftStoreChecks
{
    public static void MemoryTimesheetDraftStoreFindsWhatItSaved()
    {
        var store = TimesheetDraftStore.NewMemoryTimesheetDraftStore();
        var item = TimesheetDraft.NewTimesheetDraft("code-799", "label-733");
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
        MemoryTimesheetDraftStoreFindsWhatItSaved();
    }
}
