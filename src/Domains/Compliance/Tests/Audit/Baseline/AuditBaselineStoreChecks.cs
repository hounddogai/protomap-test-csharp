namespace Acme.Workforce.Compliance.Tests.Audit.Baseline;

using Acme.Workforce.Compliance.Audit.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AuditBaselineStoreChecks
{
    public static void MemoryAuditBaselineStoreFindsWhatItSaved()
    {
        var store = AuditBaselineStore.NewMemoryAuditBaselineStore();
        var item = AuditBaseline.NewAuditBaseline("code-367", "name-68");
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
        MemoryAuditBaselineStoreFindsWhatItSaved();
    }
}
