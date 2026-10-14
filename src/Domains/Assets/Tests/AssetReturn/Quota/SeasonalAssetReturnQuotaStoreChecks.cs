namespace Acme.Workforce.Assets.Tests.AssetReturn.Quota;

using Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class SeasonalAssetReturnQuotaStoreChecks
{
    public static void MemorySeasonalAssetReturnQuotaStoreFindsWhatItSaved()
    {
        var store = SeasonalAssetReturnQuotaStore.NewMemorySeasonalAssetReturnQuotaStore();
        var item = SeasonalAssetReturnQuota.NewSeasonalAssetReturnQuota("code-96", "title-556");
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
        MemorySeasonalAssetReturnQuotaStoreFindsWhatItSaved();
    }
}
