namespace Acme.Workforce.Absence.Tests.Balance.Summary;

using Acme.Workforce.Absence.Balance.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingBalanceAuditStoreChecks
{
    public static void MemoryPendingBalanceAuditStoreFindsWhatItSaved()
    {
        var store = PendingBalanceAuditStore.NewMemoryPendingBalanceAuditStore();
        var item = PendingBalanceAudit.NewPendingBalanceAudit("code-329", "label-32");
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
        MemoryPendingBalanceAuditStoreFindsWhatItSaved();
    }
}
