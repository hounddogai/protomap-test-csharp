namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatReservation;

using Acme.Workforce.Assets.LicenseSeat.SeatReservation;

/// <summary>Checks of seat reservation code, run by the project's runner.</summary>
public static class LicenseSeatWindowStoreChecks
{
    public static void MemoryLicenseSeatWindowStoreFindsWhatItSaved()
    {
        var store = LicenseSeatWindowStore.NewMemoryLicenseSeatWindowStore();
        var item = LicenseSeatWindow.NewLicenseSeatWindow("code-507", "title-789");
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
        MemoryLicenseSeatWindowStoreFindsWhatItSaved();
    }
}
