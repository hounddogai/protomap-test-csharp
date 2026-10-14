namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class AssetReturnProfileStoreChecks
{
    public static void MemoryAssetReturnProfileStoreFindsWhatItSaved()
    {
        var store = AssetReturnProfileStore.NewMemoryAssetReturnProfileStore();
        var item = AssetReturnProfile.NewAssetReturnProfile("code-443", "title-797");
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
        MemoryAssetReturnProfileStoreFindsWhatItSaved();
    }
}
