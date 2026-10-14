namespace Acme.Workforce.Contractors.Tests.Rate.Tier;

using Acme.Workforce.Contractors.Rate.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class RateLedgerStoreChecks
{
    public static void MemoryRateLedgerStoreFindsWhatItSaved()
    {
        var store = RateLedgerStore.NewMemoryRateLedgerStore();
        var item = RateLedger.NewRateLedger("code-701", "name-229");
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
        MemoryRateLedgerStoreFindsWhatItSaved();
    }
}
