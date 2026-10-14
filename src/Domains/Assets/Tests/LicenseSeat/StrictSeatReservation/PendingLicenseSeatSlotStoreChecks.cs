namespace Acme.Workforce.Assets.Tests.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

/// <summary>Checks of strict seat reservation code, run by the project's runner.</summary>
public static class PendingLicenseSeatSlotStoreChecks
{
    public static void MemoryPendingLicenseSeatSlotStoreFindsWhatItSaved()
    {
        var store = PendingLicenseSeatSlotStore.NewMemoryPendingLicenseSeatSlotStore();
        var item = PendingLicenseSeatSlot.NewPendingLicenseSeatSlot("code-752", "label-510");
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
        MemoryPendingLicenseSeatSlotStoreFindsWhatItSaved();
    }
}
