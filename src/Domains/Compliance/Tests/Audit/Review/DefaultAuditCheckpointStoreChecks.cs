namespace Acme.Workforce.Compliance.Tests.Audit.Review;

using Acme.Workforce.Compliance.Audit.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class DefaultAuditCheckpointStoreChecks
{
    public static void MemoryDefaultAuditCheckpointStoreFindsWhatItSaved()
    {
        var store = DefaultAuditCheckpointStore.NewMemoryDefaultAuditCheckpointStore();
        var item = DefaultAuditCheckpoint.NewDefaultAuditCheckpoint("code-816", "name-623");
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
        MemoryDefaultAuditCheckpointStoreFindsWhatItSaved();
    }
}
