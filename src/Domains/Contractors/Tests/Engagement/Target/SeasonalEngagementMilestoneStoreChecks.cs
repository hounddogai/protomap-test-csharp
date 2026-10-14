namespace Acme.Workforce.Contractors.Tests.Engagement.Target;

using Acme.Workforce.Contractors.Engagement.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class SeasonalEngagementMilestoneStoreChecks
{
    public static void MemorySeasonalEngagementMilestoneStoreFindsWhatItSaved()
    {
        var store = SeasonalEngagementMilestoneStore.NewMemorySeasonalEngagementMilestoneStore();
        var item = SeasonalEngagementMilestone.NewSeasonalEngagementMilestone("code-958", "label-100");
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
        MemorySeasonalEngagementMilestoneStoreFindsWhatItSaved();
    }
}
