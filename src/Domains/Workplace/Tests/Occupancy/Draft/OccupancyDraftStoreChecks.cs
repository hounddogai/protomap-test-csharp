namespace Acme.Workforce.Workplace.Tests.Occupancy.Draft;

using Acme.Workforce.Workplace.Occupancy.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class OccupancyDraftStoreChecks
{
    public static void MemoryOccupancyDraftStoreFindsWhatItSaved()
    {
        var store = OccupancyDraftStore.NewMemoryOccupancyDraftStore();
        var item = OccupancyDraft.NewOccupancyDraft("code-831", "label-508");
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
        MemoryOccupancyDraftStoreFindsWhatItSaved();
    }
}
