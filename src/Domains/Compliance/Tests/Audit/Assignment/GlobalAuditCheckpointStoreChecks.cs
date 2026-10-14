namespace Acme.Workforce.Compliance.Tests.Audit.Assignment;

using Acme.Workforce.Compliance.Audit.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class GlobalAuditCheckpointStoreChecks
{
    public static void MemoryGlobalAuditCheckpointStoreFindsWhatItSaved()
    {
        var store = GlobalAuditCheckpointStore.NewMemoryGlobalAuditCheckpointStore();
        var item = GlobalAuditCheckpoint.NewGlobalAuditCheckpoint("code-745", "name-544");
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
        MemoryGlobalAuditCheckpointStoreFindsWhatItSaved();
    }
}
