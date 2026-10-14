namespace Acme.Workforce.Assets.Tests.Laptop.LaptopAdjustment;

using Acme.Workforce.Assets.Laptop.LaptopAdjustment;

/// <summary>Checks of laptop adjustment code, run by the project's runner.</summary>
public static class CappedLaptopRecordStoreChecks
{
    public static void MemoryCappedLaptopRecordStoreFindsWhatItSaved()
    {
        var store = CappedLaptopRecordStore.NewMemoryCappedLaptopRecordStore();
        var item = CappedLaptopRecord.NewCappedLaptopRecord("code-941", "name-67");
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
        MemoryCappedLaptopRecordStoreFindsWhatItSaved();
    }
}
