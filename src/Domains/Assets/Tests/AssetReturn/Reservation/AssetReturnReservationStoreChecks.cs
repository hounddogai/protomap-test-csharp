namespace Acme.Workforce.Assets.Tests.AssetReturn.Reservation;

using Acme.Workforce.Assets.AssetReturn.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class AssetReturnReservationStoreChecks
{
    public static void MemoryAssetReturnReservationStoreFindsWhatItSaved()
    {
        var store = AssetReturnReservationStore.NewMemoryAssetReturnReservationStore();
        var item = AssetReturnReservation.NewAssetReturnReservation("code-286", "label-900");
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
        MemoryAssetReturnReservationStoreFindsWhatItSaved();
    }
}
