namespace Acme.Workforce.Contractors.Tests.Engagement.EngagementSummary;

using Acme.Workforce.Contractors.Engagement.EngagementSummary;

/// <summary>Checks of engagement summary code, run by the project's runner.</summary>
public static class RollingEngagementForecastStoreChecks
{
    public static void MemoryRollingEngagementForecastStoreFindsWhatItSaved()
    {
        var store = RollingEngagementForecastStore.NewMemoryRollingEngagementForecastStore();
        var item = RollingEngagementForecast.NewRollingEngagementForecast("code-902", "label-83");
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
        MemoryRollingEngagementForecastStoreFindsWhatItSaved();
    }
}
