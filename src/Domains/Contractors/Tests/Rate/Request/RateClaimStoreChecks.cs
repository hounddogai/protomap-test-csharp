namespace Acme.Workforce.Contractors.Tests.Rate.Request;

using Acme.Workforce.Contractors.Rate.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RateClaimStoreChecks
{
    public static void MemoryRateClaimStoreFindsWhatItSaved()
    {
        var store = RateClaimStore.NewMemoryRateClaimStore();
        var item = RateClaim.NewRateClaim("code-259", "name-422");
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
        MemoryRateClaimStoreFindsWhatItSaved();
    }
}
