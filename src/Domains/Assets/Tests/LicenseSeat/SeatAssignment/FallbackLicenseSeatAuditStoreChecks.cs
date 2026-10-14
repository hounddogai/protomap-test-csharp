namespace Acme.Workforce.Assets.Tests.LicenseSeat.SeatAssignment;

using Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

/// <summary>Checks of seat assignment code, run by the project's runner.</summary>
public static class FallbackLicenseSeatAuditStoreChecks
{
    public static void MemoryFallbackLicenseSeatAuditStoreFindsWhatItSaved()
    {
        var store = FallbackLicenseSeatAuditStore.NewMemoryFallbackLicenseSeatAuditStore();
        var item = FallbackLicenseSeatAudit.NewFallbackLicenseSeatAudit("code-313", "name-298");
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
        MemoryFallbackLicenseSeatAuditStoreFindsWhatItSaved();
    }
}
