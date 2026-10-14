namespace Acme.Workforce.Assets.Tests.LicenseSeat.Audit;

using Acme.Workforce.Assets.LicenseSeat.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class LicenseSeatAuditStoreChecks
{
    public static void MemoryLicenseSeatAuditStoreFindsWhatItSaved()
    {
        var store = LicenseSeatAuditStore.NewMemoryLicenseSeatAuditStore();
        var item = LicenseSeatAudit.NewLicenseSeatAudit("code-686", "title-16");
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
        MemoryLicenseSeatAuditStoreFindsWhatItSaved();
    }
}
