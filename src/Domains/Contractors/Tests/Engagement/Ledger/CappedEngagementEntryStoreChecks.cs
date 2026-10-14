namespace Acme.Workforce.Contractors.Tests.Engagement.Ledger;

using Acme.Workforce.Contractors.Engagement.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class CappedEngagementEntryStoreChecks
{
    public static void MemoryCappedEngagementEntryStoreFindsWhatItSaved()
    {
        var store = CappedEngagementEntryStore.NewMemoryCappedEngagementEntryStore();
        var item = CappedEngagementEntry.NewCappedEngagementEntry("code-528", "label-180");
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
        MemoryCappedEngagementEntryStoreFindsWhatItSaved();
    }
}
