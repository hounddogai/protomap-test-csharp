namespace Acme.Workforce.Compliance.Tests.Audit.AuditRecord;

using Acme.Workforce.Compliance.Audit.AuditRecord;

/// <summary>Checks of audit record code, run by the project's runner.</summary>
public static class CappedAuditCycleStoreChecks
{
    public static void MemoryCappedAuditCycleStoreFindsWhatItSaved()
    {
        var store = CappedAuditCycleStore.NewMemoryCappedAuditCycleStore();
        var item = CappedAuditCycle.NewCappedAuditCycle("code-770", "label-565");
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
        MemoryCappedAuditCycleStoreFindsWhatItSaved();
    }
}
