namespace Acme.Workforce.Contractors.Tests.Engagement.Notice;

using Acme.Workforce.Contractors.Engagement.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class SeasonalEngagementRollupStoreChecks
{
    public static void MemorySeasonalEngagementRollupStoreFindsWhatItSaved()
    {
        var store = SeasonalEngagementRollupStore.NewMemorySeasonalEngagementRollupStore();
        var item = SeasonalEngagementRollup.NewSeasonalEngagementRollup("code-601", "title-56");
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
        MemorySeasonalEngagementRollupStoreFindsWhatItSaved();
    }
}
