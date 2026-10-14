namespace Acme.Workforce.Workplace.Tests.AccessBadge.Ledger;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class RegionalAccessBadgeLedgerStoreChecks
{
    public static void MemoryRegionalAccessBadgeLedgerStoreFindsWhatItSaved()
    {
        var store = RegionalAccessBadgeLedgerStore.NewMemoryRegionalAccessBadgeLedgerStore();
        var item = RegionalAccessBadgeLedger.NewRegionalAccessBadgeLedger("code-78", "label-959");
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
        MemoryRegionalAccessBadgeLedgerStoreFindsWhatItSaved();
    }
}
