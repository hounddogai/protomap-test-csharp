namespace Acme.Workforce.Absence.Tests.Balance.Ledger;

using Acme.Workforce.Absence.Balance.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class BalanceLedgerStoreChecks
{
    public static void MemoryBalanceLedgerStoreFindsWhatItSaved()
    {
        var store = BalanceLedgerStore.NewMemoryBalanceLedgerStore();
        var item = BalanceLedger.NewBalanceLedger("code-500", "label-216");
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
        MemoryBalanceLedgerStoreFindsWhatItSaved();
    }
}
