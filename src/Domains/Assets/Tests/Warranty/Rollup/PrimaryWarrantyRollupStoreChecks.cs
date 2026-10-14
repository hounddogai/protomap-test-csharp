namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryWarrantyRollupStoreChecks
{
    public static void MemoryPrimaryWarrantyRollupStoreFindsWhatItSaved()
    {
        var store = PrimaryWarrantyRollupStore.NewMemoryPrimaryWarrantyRollupStore();
        var item = PrimaryWarrantyRollup.NewPrimaryWarrantyRollup("code-977", "title-133");
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
        MemoryPrimaryWarrantyRollupStoreFindsWhatItSaved();
    }
}
