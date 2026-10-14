namespace Acme.Workforce.Assets.Tests.LicenseSeat.Agreement;

using Acme.Workforce.Assets.LicenseSeat.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LenientLicenseSeatScheduleStoreChecks
{
    public static void MemoryLenientLicenseSeatScheduleStoreFindsWhatItSaved()
    {
        var store = LenientLicenseSeatScheduleStore.NewMemoryLenientLicenseSeatScheduleStore();
        var item = LenientLicenseSeatSchedule.NewLenientLicenseSeatSchedule("code-531", "label-125");
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
        MemoryLenientLicenseSeatScheduleStoreFindsWhatItSaved();
    }
}
