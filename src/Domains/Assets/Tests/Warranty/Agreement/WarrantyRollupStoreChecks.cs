namespace Acme.Workforce.Assets.Tests.Warranty.Agreement;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class WarrantyRollupStoreChecks
{
    public static void MemoryWarrantyRollupStoreFindsWhatItSaved()
    {
        var store = WarrantyRollupStore.NewMemoryWarrantyRollupStore();
        var item = WarrantyRollup.NewWarrantyRollup("code-798", "name-446");
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
        MemoryWarrantyRollupStoreFindsWhatItSaved();
    }
}
