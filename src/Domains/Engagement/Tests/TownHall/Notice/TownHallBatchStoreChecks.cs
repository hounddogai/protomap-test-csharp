namespace Acme.Workforce.Engagement.Tests.TownHall.Notice;

using Acme.Workforce.Engagement.TownHall.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class TownHallBatchStoreChecks
{
    public static void MemoryTownHallBatchStoreFindsWhatItSaved()
    {
        var store = TownHallBatchStore.NewMemoryTownHallBatchStore();
        var item = TownHallBatch.NewTownHallBatch("code-474", "title-779");
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
        MemoryTownHallBatchStoreFindsWhatItSaved();
    }
}
