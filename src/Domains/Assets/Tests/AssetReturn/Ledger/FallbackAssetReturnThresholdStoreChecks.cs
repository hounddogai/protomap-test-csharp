namespace Acme.Workforce.Assets.Tests.AssetReturn.Ledger;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class FallbackAssetReturnThresholdStoreChecks
{
    public static void MemoryFallbackAssetReturnThresholdStoreFindsWhatItSaved()
    {
        var store = FallbackAssetReturnThresholdStore.NewMemoryFallbackAssetReturnThresholdStore();
        var item = FallbackAssetReturnThreshold.NewFallbackAssetReturnThreshold("code-354", "label-466");
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
        MemoryFallbackAssetReturnThresholdStoreFindsWhatItSaved();
    }
}
