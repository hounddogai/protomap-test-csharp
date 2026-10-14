namespace Acme.Workforce.Workplace.Tests.AccessBadge.BadgeEntry;

using Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

/// <summary>Checks of badge entry code, run by the project's runner.</summary>
public static class RegionalAccessBadgeScoreStoreChecks
{
    public static void MemoryRegionalAccessBadgeScoreStoreFindsWhatItSaved()
    {
        var store = RegionalAccessBadgeScoreStore.NewMemoryRegionalAccessBadgeScoreStore();
        var item = RegionalAccessBadgeScore.NewRegionalAccessBadgeScore("code-700", "name-363");
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
        MemoryRegionalAccessBadgeScoreStoreFindsWhatItSaved();
    }
}
