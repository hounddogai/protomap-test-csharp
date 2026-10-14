namespace Acme.Workforce.Compliance.Tests.Audit.Checkpoint;

using Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class AuditClaimStoreChecks
{
    public static void MemoryAuditClaimStoreFindsWhatItSaved()
    {
        var store = AuditClaimStore.NewMemoryAuditClaimStore();
        var item = AuditClaim.NewAuditClaim("code-761", "name-660");
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
        MemoryAuditClaimStoreFindsWhatItSaved();
    }
}
