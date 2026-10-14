namespace Acme.Workforce.Workplace.Tests.Occupancy.Digest;

using Acme.Workforce.Workplace.Occupancy.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class TieredOccupancyRecordStoreChecks
{
    public static void MemoryTieredOccupancyRecordStoreFindsWhatItSaved()
    {
        var store = TieredOccupancyRecordStore.NewMemoryTieredOccupancyRecordStore();
        var item = TieredOccupancyRecord.NewTieredOccupancyRecord("code-596", "label-939");
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
        MemoryTieredOccupancyRecordStoreFindsWhatItSaved();
    }
}
