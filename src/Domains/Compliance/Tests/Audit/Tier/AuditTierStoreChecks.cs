namespace Acme.Workforce.Compliance.Tests.Audit.Tier;

using Acme.Workforce.Compliance.Audit.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AuditTierStoreChecks
{
    public static void MemoryAuditTierStoreFindsWhatItSaved()
    {
        var store = AuditTierStore.NewMemoryAuditTierStore();
        var item = AuditTier.NewAuditTier("code-494", "title-434");
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
        MemoryAuditTierStoreFindsWhatItSaved();
    }
}
