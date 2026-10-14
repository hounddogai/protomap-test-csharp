namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class LaptopAdjustmentStoreChecks
{
    public static void MemoryLaptopAdjustmentStoreFindsWhatItSaved()
    {
        var store = LaptopAdjustmentStore.NewMemoryLaptopAdjustmentStore();
        var item = LaptopAdjustment.NewLaptopAdjustment("code-881", "title-835");
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
        MemoryLaptopAdjustmentStoreFindsWhatItSaved();
    }
}
