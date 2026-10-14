namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class BatchedTownHallNoticeStoreChecks
{
    public static void MemoryBatchedTownHallNoticeStoreFindsWhatItSaved()
    {
        var store = BatchedTownHallNoticeStore.NewMemoryBatchedTownHallNoticeStore();
        var item = BatchedTownHallNotice.NewBatchedTownHallNotice("code-911", "name-594");
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
        MemoryBatchedTownHallNoticeStoreFindsWhatItSaved();
    }
}
