namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingReorgEntryStoreChecks
{
    public static void MemoryPendingReorgEntryStoreFindsWhatItSaved()
    {
        var store = PendingReorgEntryStore.NewMemoryPendingReorgEntryStore();
        var item = PendingReorgEntry.NewPendingReorgEntry("code-206", "title-841");
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
        MemoryPendingReorgEntryStoreFindsWhatItSaved();
    }
}
