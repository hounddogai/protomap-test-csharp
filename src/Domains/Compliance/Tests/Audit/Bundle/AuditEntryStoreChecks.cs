namespace Acme.Workforce.Compliance.Tests.Audit.Bundle;

using Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class AuditEntryStoreChecks
{
    public static void MemoryAuditEntryStoreFindsWhatItSaved()
    {
        var store = AuditEntryStore.NewMemoryAuditEntryStore();
        var item = AuditEntry.NewAuditEntry("code-356", "label-61");
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
        MemoryAuditEntryStoreFindsWhatItSaved();
    }
}
