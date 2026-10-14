namespace Acme.Workforce.Contractors.Tests.Engagement.Profile;

using Acme.Workforce.Contractors.Engagement.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class EngagementMetricStoreChecks
{
    public static void MemoryEngagementMetricStoreFindsWhatItSaved()
    {
        var store = EngagementMetricStore.NewMemoryEngagementMetricStore();
        var item = EngagementMetric.NewEngagementMetric("code-858", "name-137");
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
        MemoryEngagementMetricStoreFindsWhatItSaved();
    }
}
