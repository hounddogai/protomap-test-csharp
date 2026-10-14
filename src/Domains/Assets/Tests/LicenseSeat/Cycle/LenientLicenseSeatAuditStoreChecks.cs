namespace Acme.Workforce.Assets.Tests.LicenseSeat.Cycle;

using Acme.Workforce.Assets.LicenseSeat.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class LenientLicenseSeatAuditStoreChecks
{
    public static void MemoryLenientLicenseSeatAuditStoreFindsWhatItSaved()
    {
        var store = LenientLicenseSeatAuditStore.NewMemoryLenientLicenseSeatAuditStore();
        var item = LenientLicenseSeatAudit.NewLenientLicenseSeatAudit("code-116", "title-916");
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
        MemoryLenientLicenseSeatAuditStoreFindsWhatItSaved();
    }
}
