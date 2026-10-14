namespace Acme.Workforce.Workplace.Tests.AccessBadge.Estimate;

using Acme.Workforce.Workplace.AccessBadge.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class GlobalAccessBadgeEstimateStoreChecks
{
    public static void MemoryGlobalAccessBadgeEstimateStoreFindsWhatItSaved()
    {
        var store = GlobalAccessBadgeEstimateStore.NewMemoryGlobalAccessBadgeEstimateStore();
        var item = GlobalAccessBadgeEstimate.NewGlobalAccessBadgeEstimate("code-482", "name-172");
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
        MemoryGlobalAccessBadgeEstimateStoreFindsWhatItSaved();
    }
}
