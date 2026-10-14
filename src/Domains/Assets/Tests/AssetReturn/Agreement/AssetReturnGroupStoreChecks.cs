namespace Acme.Workforce.Assets.Tests.AssetReturn.Agreement;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssetReturnGroupStoreChecks
{
    public static void MemoryAssetReturnGroupStoreFindsWhatItSaved()
    {
        var store = AssetReturnGroupStore.NewMemoryAssetReturnGroupStore();
        var item = AssetReturnGroup.NewAssetReturnGroup("code-724", "title-470");
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
        MemoryAssetReturnGroupStoreFindsWhatItSaved();
    }
}
