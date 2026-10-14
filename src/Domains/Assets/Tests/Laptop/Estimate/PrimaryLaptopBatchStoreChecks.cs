namespace Acme.Workforce.Assets.Tests.Laptop.Estimate;

using Acme.Workforce.Assets.Laptop.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class PrimaryLaptopBatchStoreChecks
{
    public static void MemoryPrimaryLaptopBatchStoreFindsWhatItSaved()
    {
        var store = PrimaryLaptopBatchStore.NewMemoryPrimaryLaptopBatchStore();
        var item = PrimaryLaptopBatch.NewPrimaryLaptopBatch("code-533", "label-700");
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
        MemoryPrimaryLaptopBatchStoreFindsWhatItSaved();
    }
}
