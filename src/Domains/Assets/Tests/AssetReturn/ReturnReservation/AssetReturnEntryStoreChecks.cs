namespace Acme.Workforce.Assets.Tests.AssetReturn.ReturnReservation;

using Acme.Workforce.Assets.AssetReturn.ReturnReservation;

/// <summary>Checks of return reservation code, run by the project's runner.</summary>
public static class AssetReturnEntryStoreChecks
{
    public static void MemoryAssetReturnEntryStoreFindsWhatItSaved()
    {
        var store = AssetReturnEntryStore.NewMemoryAssetReturnEntryStore();
        var item = AssetReturnEntry.NewAssetReturnEntry("code-284", "title-512");
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
        MemoryAssetReturnEntryStoreFindsWhatItSaved();
    }
}
