namespace Acme.Workforce.Assets.Tests.AssetReturn.Quota;

using Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class AssetReturnClaimStoreChecks
{
    public static void MemoryAssetReturnClaimStoreFindsWhatItSaved()
    {
        var store = AssetReturnClaimStore.NewMemoryAssetReturnClaimStore();
        var item = AssetReturnClaim.NewAssetReturnClaim("code-707", "label-816");
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
        MemoryAssetReturnClaimStoreFindsWhatItSaved();
    }
}
