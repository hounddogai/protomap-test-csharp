namespace Acme.Workforce.Assets.Tests.AssetReturn.Profile;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class StrictAssetReturnTierStoreChecks
{
    public static void MemoryStrictAssetReturnTierStoreFindsWhatItSaved()
    {
        var store = StrictAssetReturnTierStore.NewMemoryStrictAssetReturnTierStore();
        var item = StrictAssetReturnTier.NewStrictAssetReturnTier("code-492", "label-186");
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
        MemoryStrictAssetReturnTierStoreFindsWhatItSaved();
    }
}
