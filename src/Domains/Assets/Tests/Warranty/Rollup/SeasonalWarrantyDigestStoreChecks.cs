namespace Acme.Workforce.Assets.Tests.Warranty.Rollup;

using Acme.Workforce.Assets.Warranty.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class SeasonalWarrantyDigestStoreChecks
{
    public static void MemorySeasonalWarrantyDigestStoreFindsWhatItSaved()
    {
        var store = SeasonalWarrantyDigestStore.NewMemorySeasonalWarrantyDigestStore();
        var item = SeasonalWarrantyDigest.NewSeasonalWarrantyDigest("code-946", "title-206");
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
        MemorySeasonalWarrantyDigestStoreFindsWhatItSaved();
    }
}
