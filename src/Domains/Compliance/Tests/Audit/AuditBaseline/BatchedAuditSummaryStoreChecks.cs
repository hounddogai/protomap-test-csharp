namespace Acme.Workforce.Compliance.Tests.Audit.AuditBaseline;

using Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Checks of audit baseline code, run by the project's runner.</summary>
public static class BatchedAuditSummaryStoreChecks
{
    public static void MemoryBatchedAuditSummaryStoreFindsWhatItSaved()
    {
        var store = BatchedAuditSummaryStore.NewMemoryBatchedAuditSummaryStore();
        var item = BatchedAuditSummary.NewBatchedAuditSummary("code-790", "label-893");
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
        MemoryBatchedAuditSummaryStoreFindsWhatItSaved();
    }
}
