namespace Acme.Workforce.Contractors.Tests.Rate.Notice;

using Acme.Workforce.Contractors.Rate.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RatePolicyStoreChecks
{
    public static void MemoryRatePolicyStoreFindsWhatItSaved()
    {
        var store = RatePolicyStore.NewMemoryRatePolicyStore();
        var item = RatePolicy.NewRatePolicy("code-79", "label-446");
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
        MemoryRatePolicyStoreFindsWhatItSaved();
    }
}
