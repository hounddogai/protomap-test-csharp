namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class LicenseSeatQuotaStoreChecks
{
    public static void MemoryLicenseSeatQuotaStoreFindsWhatItSaved()
    {
        var store = LicenseSeatQuotaStore.NewMemoryLicenseSeatQuotaStore();
        var item = LicenseSeatQuota.NewLicenseSeatQuota("code-259", "name-776");
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
        MemoryLicenseSeatQuotaStoreFindsWhatItSaved();
    }
}
