namespace Acme.Workforce.Payrollops.Tests.Garnishment.Ledger;

using Acme.Workforce.Payrollops.Garnishment.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class GarnishmentLedgerStoreChecks
{
    public static void MemoryGarnishmentLedgerStoreFindsWhatItSaved()
    {
        var store = GarnishmentLedgerStore.NewMemoryGarnishmentLedgerStore();
        var item = GarnishmentLedger.NewGarnishmentLedger("code-673", "name-218");
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
        MemoryGarnishmentLedgerStoreFindsWhatItSaved();
    }
}
