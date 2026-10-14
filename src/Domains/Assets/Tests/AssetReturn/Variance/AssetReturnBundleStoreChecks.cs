namespace Acme.Workforce.Assets.Tests.AssetReturn.Variance;

using Acme.Workforce.Assets.AssetReturn.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AssetReturnBundleStoreChecks
{
    public static void MemoryAssetReturnBundleStoreFindsWhatItSaved()
    {
        var store = AssetReturnBundleStore.NewMemoryAssetReturnBundleStore();
        var item = AssetReturnBundle.NewAssetReturnBundle("code-188", "name-628");
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
        MemoryAssetReturnBundleStoreFindsWhatItSaved();
    }
}
