namespace Acme.Workforce.Workplace.Tests.Desk.Preference;

using Acme.Workforce.Workplace.Desk.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class ScheduledDeskSummaryStoreChecks
{
    public static void MemoryScheduledDeskSummaryStoreFindsWhatItSaved()
    {
        var store = ScheduledDeskSummaryStore.NewMemoryScheduledDeskSummaryStore();
        var item = ScheduledDeskSummary.NewScheduledDeskSummary("code-698", "title-469");
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
        MemoryScheduledDeskSummaryStoreFindsWhatItSaved();
    }
}
