namespace Acme.Workforce.Engagement.Tests.TownHall.Request;

using Acme.Workforce.Engagement.TownHall.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdaptiveTownHallLedgerStoreChecks
{
    public static void MemoryAdaptiveTownHallLedgerStoreFindsWhatItSaved()
    {
        var store = AdaptiveTownHallLedgerStore.NewMemoryAdaptiveTownHallLedgerStore();
        var item = AdaptiveTownHallLedger.NewAdaptiveTownHallLedger("code-324", "name-96");
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
        MemoryAdaptiveTownHallLedgerStoreFindsWhatItSaved();
    }
}
