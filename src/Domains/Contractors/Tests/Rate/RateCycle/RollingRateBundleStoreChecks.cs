namespace Acme.Workforce.Contractors.Tests.Rate.RateCycle;

using Acme.Workforce.Contractors.Rate.RateCycle;

/// <summary>Checks of rate cycle code, run by the project's runner.</summary>
public static class RollingRateBundleStoreChecks
{
    public static void MemoryRollingRateBundleStoreFindsWhatItSaved()
    {
        var store = RollingRateBundleStore.NewMemoryRollingRateBundleStore();
        var item = RollingRateBundle.NewRollingRateBundle("code-508", "name-918");
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
        MemoryRollingRateBundleStoreFindsWhatItSaved();
    }
}
