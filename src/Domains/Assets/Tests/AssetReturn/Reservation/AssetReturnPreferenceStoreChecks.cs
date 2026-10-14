namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnPreferenceStoreChecks
{
    public static void MemoryAssetReturnPreferenceStoreFindsWhatItSaved()
    {
        var store = AssetReturnPreferenceStore.NewMemoryAssetReturnPreferenceStore();
        var item = AssetReturnPreference.NewAssetReturnPreference("code-672", "label-276");
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
        MemoryAssetReturnPreferenceStoreFindsWhatItSaved();
    }
}
