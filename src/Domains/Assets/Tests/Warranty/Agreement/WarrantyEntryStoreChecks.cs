namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyEntryStoreChecks
{
    public static void MemoryWarrantyEntryStoreFindsWhatItSaved()
    {
        var store = WarrantyEntryStore.NewMemoryWarrantyEntryStore();
        var item = WarrantyEntry.NewWarrantyEntry("code-609", "label-907");
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
        MemoryWarrantyEntryStoreFindsWhatItSaved();
    }
}
