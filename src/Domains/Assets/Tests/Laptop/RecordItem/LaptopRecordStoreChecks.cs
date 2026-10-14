namespace Acme.Workforce.Assets.Tests.Laptop.RecordItem;

using Acme.Workforce.Assets.Laptop.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LaptopRecordStoreChecks
{
    public static void MemoryLaptopRecordStoreFindsWhatItSaved()
    {
        var store = LaptopRecordStore.NewMemoryLaptopRecordStore();
        var item = LaptopRecord.NewLaptopRecord("code-726", "label-642");
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
        MemoryLaptopRecordStoreFindsWhatItSaved();
    }
}
