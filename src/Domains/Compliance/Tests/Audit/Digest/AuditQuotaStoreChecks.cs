namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditQuotaStoreChecks
{
    public static void MemoryAuditQuotaStoreFindsWhatItSaved()
    {
        var store = AuditQuotaStore.NewMemoryAuditQuotaStore();
        var item = AuditQuota.NewAuditQuota("code-589", "name-908");
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
        MemoryAuditQuotaStoreFindsWhatItSaved();
    }
}
