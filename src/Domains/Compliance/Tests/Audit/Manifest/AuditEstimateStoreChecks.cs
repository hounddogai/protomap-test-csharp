namespace Acme.Workforce.Compliance.Tests.Audit.Manifest;

using Acme.Workforce.Compliance.Audit.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AuditEstimateStoreChecks
{
    public static void MemoryAuditEstimateStoreFindsWhatItSaved()
    {
        var store = AuditEstimateStore.NewMemoryAuditEstimateStore();
        var item = AuditEstimate.NewAuditEstimate("code-527", "name-10");
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
        MemoryAuditEstimateStoreFindsWhatItSaved();
    }
}
