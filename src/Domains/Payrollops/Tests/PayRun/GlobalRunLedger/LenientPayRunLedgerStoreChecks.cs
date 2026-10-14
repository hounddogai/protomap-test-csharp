namespace Acme.Workforce.Payrollops.Tests.PayRun.GlobalRunLedger;

using Acme.Workforce.Payrollops.PayRun.GlobalRunLedger;

/// <summary>Checks of global run ledger code, run by the project's runner.</summary>
public static class LenientPayRunLedgerStoreChecks
{
    public static void MemoryLenientPayRunLedgerStoreFindsWhatItSaved()
    {
        var store = LenientPayRunLedgerStore.NewMemoryLenientPayRunLedgerStore();
        var item = LenientPayRunLedger.NewLenientPayRunLedger("code-975", "name-288");
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
        MemoryLenientPayRunLedgerStoreFindsWhatItSaved();
    }
}
