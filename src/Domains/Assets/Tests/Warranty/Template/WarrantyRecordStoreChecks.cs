namespace Acme.Workforce.Assets.Tests.Warranty.Template;

using Acme.Workforce.Assets.Warranty.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class WarrantyRecordStoreChecks
{
    public static void MemoryWarrantyRecordStoreFindsWhatItSaved()
    {
        var store = WarrantyRecordStore.NewMemoryWarrantyRecordStore();
        var item = WarrantyRecord.NewWarrantyRecord("code-585", "title-639");
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
        MemoryWarrantyRecordStoreFindsWhatItSaved();
    }
}
