namespace Acme.Workforce.Contractors.Tests.Rate.RateProposal;

using Acme.Workforce.Contractors.Rate.RateProposal;

/// <summary>Checks of rate proposal code, run by the project's runner.</summary>
public static class PendingRateWindowStoreChecks
{
    public static void MemoryPendingRateWindowStoreFindsWhatItSaved()
    {
        var store = PendingRateWindowStore.NewMemoryPendingRateWindowStore();
        var item = PendingRateWindow.NewPendingRateWindow("code-805", "title-100");
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
        MemoryPendingRateWindowStoreFindsWhatItSaved();
    }
}
