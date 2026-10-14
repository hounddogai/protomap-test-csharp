namespace Acme.Workforce.Assets.Tests.Refresh.Ledger;

using Acme.Workforce.Assets.Refresh.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RefreshLedgerStoreChecks
{
    public static void MemoryRefreshLedgerStoreFindsWhatItSaved()
    {
        var store = RefreshLedgerStore.NewMemoryRefreshLedgerStore();
        var item = RefreshLedger.NewRefreshLedger("code-999", "label-741");
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
        MemoryRefreshLedgerStoreFindsWhatItSaved();
    }
}
