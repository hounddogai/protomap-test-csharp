namespace Acme.Workforce.Assets.Tests.Warranty.Checkpoint;

using Acme.Workforce.Assets.Warranty.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class WarrantyCheckpointStoreChecks
{
    public static void MemoryWarrantyCheckpointStoreFindsWhatItSaved()
    {
        var store = WarrantyCheckpointStore.NewMemoryWarrantyCheckpointStore();
        var item = WarrantyCheckpoint.NewWarrantyCheckpoint("code-223", "title-507");
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
        MemoryWarrantyCheckpointStoreFindsWhatItSaved();
    }
}
