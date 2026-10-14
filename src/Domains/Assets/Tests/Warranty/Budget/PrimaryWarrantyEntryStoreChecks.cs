namespace Acme.Workforce.Assets.Tests.Warranty.Budget;

using Acme.Workforce.Assets.Warranty.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class PrimaryWarrantyEntryStoreChecks
{
    public static void MemoryPrimaryWarrantyEntryStoreFindsWhatItSaved()
    {
        var store = PrimaryWarrantyEntryStore.NewMemoryPrimaryWarrantyEntryStore();
        var item = PrimaryWarrantyEntry.NewPrimaryWarrantyEntry("code-601", "title-474");
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
        MemoryPrimaryWarrantyEntryStoreFindsWhatItSaved();
    }
}
