namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyProfileStoreChecks
{
    public static void MemorySeasonalWarrantyProfileStoreFindsWhatItSaved()
    {
        var store = SeasonalWarrantyProfileStore.NewMemorySeasonalWarrantyProfileStore();
        var item = SeasonalWarrantyProfile.NewSeasonalWarrantyProfile("code-620", "title-115");
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
        MemorySeasonalWarrantyProfileStoreFindsWhatItSaved();
    }
}
