namespace Acme.Workforce.Compliance.Tests.Audit.Claim;

using Acme.Workforce.Compliance.Audit.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class AuditProfileStoreChecks
{
    public static void MemoryAuditProfileStoreFindsWhatItSaved()
    {
        var store = AuditProfileStore.NewMemoryAuditProfileStore();
        var item = AuditProfile.NewAuditProfile("code-735", "label-129");
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
        MemoryAuditProfileStoreFindsWhatItSaved();
    }
}
