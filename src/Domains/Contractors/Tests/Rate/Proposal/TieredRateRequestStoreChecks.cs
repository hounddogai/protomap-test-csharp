namespace Acme.Workforce.Contractors.Tests.Rate.Proposal;

using Acme.Workforce.Contractors.Rate.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class TieredRateRequestStoreChecks
{
    public static void MemoryTieredRateRequestStoreFindsWhatItSaved()
    {
        var store = TieredRateRequestStore.NewMemoryTieredRateRequestStore();
        var item = TieredRateRequest.NewTieredRateRequest("code-890", "label-258");
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
        MemoryTieredRateRequestStoreFindsWhatItSaved();
    }
}
