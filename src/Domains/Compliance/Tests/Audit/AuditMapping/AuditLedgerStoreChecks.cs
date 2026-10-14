namespace Acme.Workforce.Compliance.Tests.Audit.AuditMapping;

using Acme.Workforce.Compliance.Audit.AuditMapping;

/// <summary>Checks of audit mapping code, run by the project's runner.</summary>
public static class AuditLedgerStoreChecks
{
    public static void MemoryAuditLedgerStoreFindsWhatItSaved()
    {
        var store = AuditLedgerStore.NewMemoryAuditLedgerStore();
        var item = AuditLedger.NewAuditLedger("code-298", "name-204");
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
        MemoryAuditLedgerStoreFindsWhatItSaved();
    }
}
