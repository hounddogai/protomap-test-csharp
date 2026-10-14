namespace Acme.Workforce.Compliance.Tests.Audit.Digest;

using Acme.Workforce.Compliance.Audit.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class AuditDigestStoreChecks
{
    public static void MemoryAuditDigestStoreFindsWhatItSaved()
    {
        var store = AuditDigestStore.NewMemoryAuditDigestStore();
        var item = AuditDigest.NewAuditDigest("code-324", "name-994");
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
        MemoryAuditDigestStoreFindsWhatItSaved();
    }
}
