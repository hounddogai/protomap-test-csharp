namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PrimaryAssetReturnProfileStoreChecks
{
    public static void MemoryPrimaryAssetReturnProfileStoreFindsWhatItSaved()
    {
        var store = PrimaryAssetReturnProfileStore.NewMemoryPrimaryAssetReturnProfileStore();
        var item = PrimaryAssetReturnProfile.NewPrimaryAssetReturnProfile("code-801", "title-594");
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
        MemoryPrimaryAssetReturnProfileStoreFindsWhatItSaved();
    }
}
