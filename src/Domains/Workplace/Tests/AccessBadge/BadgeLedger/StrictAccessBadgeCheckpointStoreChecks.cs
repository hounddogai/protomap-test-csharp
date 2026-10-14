namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class StrictAccessBadgeCheckpointStoreChecks
{
    public static void MemoryStrictAccessBadgeCheckpointStoreFindsWhatItSaved()
    {
        var store = StrictAccessBadgeCheckpointStore.NewMemoryStrictAccessBadgeCheckpointStore();
        var item = StrictAccessBadgeCheckpoint.NewStrictAccessBadgeCheckpoint("code-11", "name-889");
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
        MemoryStrictAccessBadgeCheckpointStoreFindsWhatItSaved();
    }
}
