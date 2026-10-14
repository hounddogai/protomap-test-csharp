namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeLedger;

using Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>Checks of badge ledger code, run by the project's runner.</summary>
public static class WeightedAccessBadgeLedgerStoreChecks
{
    public static void MemoryWeightedAccessBadgeLedgerStoreFindsWhatItSaved()
    {
        var store = WeightedAccessBadgeLedgerStore.NewMemoryWeightedAccessBadgeLedgerStore();
        var item = WeightedAccessBadgeLedger.NewWeightedAccessBadgeLedger("code-141", "name-831");
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
        MemoryWeightedAccessBadgeLedgerStoreFindsWhatItSaved();
    }
}
