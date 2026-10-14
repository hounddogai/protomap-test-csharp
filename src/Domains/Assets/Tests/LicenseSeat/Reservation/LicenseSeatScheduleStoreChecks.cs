namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatScheduleStoreChecks
{
    public static void MemoryLicenseSeatScheduleStoreFindsWhatItSaved()
    {
        var store = LicenseSeatScheduleStore.NewMemoryLicenseSeatScheduleStore();
        var item = LicenseSeatSchedule.NewLicenseSeatSchedule("code-723", "name-412");
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
        MemoryLicenseSeatScheduleStoreFindsWhatItSaved();
    }
}
