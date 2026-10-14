namespace Acme.Workforce.Workplace.Tests.Occupancy.Ledger;

using Acme.Workforce.Workplace.Occupancy.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class OccupancyLedgerStoreChecks
{
    public static void MemoryOccupancyLedgerStoreFindsWhatItSaved()
    {
        var store = OccupancyLedgerStore.NewMemoryOccupancyLedgerStore();
        var item = OccupancyLedger.NewOccupancyLedger("code-768", "title-140");
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
        MemoryOccupancyLedgerStoreFindsWhatItSaved();
    }
}
