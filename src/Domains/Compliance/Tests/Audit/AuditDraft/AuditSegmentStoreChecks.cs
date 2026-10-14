namespace Acme.Workforce.Compliance.Tests.Audit.AuditDraft;

using Acme.Workforce.Compliance.Audit.AuditDraft;

/// <summary>Checks of audit draft code, run by the project's runner.</summary>
public static class AuditSegmentStoreChecks
{
    public static void MemoryAuditSegmentStoreFindsWhatItSaved()
    {
        var store = AuditSegmentStore.NewMemoryAuditSegmentStore();
        var item = AuditSegment.NewAuditSegment("code-346", "title-113");
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
        MemoryAuditSegmentStoreFindsWhatItSaved();
    }
}
