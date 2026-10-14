namespace Acme.Workforce.Assets.Tests.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class AssetReturnPolicyStoreChecks
{
    public static void MemoryAssetReturnPolicyStoreFindsWhatItSaved()
    {
        var store = AssetReturnPolicyStore.NewMemoryAssetReturnPolicyStore();
        var item = AssetReturnPolicy.NewAssetReturnPolicy("code-814", "label-922");
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
        MemoryAssetReturnPolicyStoreFindsWhatItSaved();
    }
}
