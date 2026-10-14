namespace Acme.Workforce.Payrollops.Tests.PayRun.CappedRunLedger;

using Acme.Workforce.Payrollops.PayRun.CappedRunLedger;

/// <summary>Checks of capped run ledger code, run by the project's runner.</summary>
public static class DefaultPayRunLedgerStoreChecks
{
    public static void MemoryDefaultPayRunLedgerStoreFindsWhatItSaved()
    {
        var store = DefaultPayRunLedgerStore.NewMemoryDefaultPayRunLedgerStore();
        var item = DefaultPayRunLedger.NewDefaultPayRunLedger("code-455", "title-272");
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
        MemoryDefaultPayRunLedgerStoreFindsWhatItSaved();
    }
}
