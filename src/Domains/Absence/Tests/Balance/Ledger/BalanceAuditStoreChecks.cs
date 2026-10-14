namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceAuditStoreChecks
{
    public static void MemoryBalanceAuditStoreFindsWhatItSaved()
    {
        var store = BalanceAuditStore.NewMemoryBalanceAuditStore();
        var item = BalanceAudit.NewBalanceAudit("code-696", "title-336");
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
        MemoryBalanceAuditStoreFindsWhatItSaved();
    }
}
