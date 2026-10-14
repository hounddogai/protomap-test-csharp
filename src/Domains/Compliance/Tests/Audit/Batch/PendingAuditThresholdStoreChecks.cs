namespace Acme.Workforce.Compliance.Tests.Audit.Batch;

using Acme.Workforce.Compliance.Audit.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class PendingAuditThresholdStoreChecks
{
    public static void MemoryPendingAuditThresholdStoreFindsWhatItSaved()
    {
        var store = PendingAuditThresholdStore.NewMemoryPendingAuditThresholdStore();
        var item = PendingAuditThreshold.NewPendingAuditThreshold("code-417", "title-362");
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
        MemoryPendingAuditThresholdStoreFindsWhatItSaved();
    }
}
