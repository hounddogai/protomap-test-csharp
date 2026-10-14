namespace Acme.Workforce.Absence.Tests.Accrual.Ledger;

using Acme.Workforce.Absence.Accrual.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class AccrualLedgerStoreChecks
{
    public static void MemoryAccrualLedgerStoreFindsWhatItSaved()
    {
        var store = AccrualLedgerStore.NewMemoryAccrualLedgerStore();
        var item = AccrualLedger.NewAccrualLedger("code-636", "label-913");
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
        MemoryAccrualLedgerStoreFindsWhatItSaved();
    }
}
