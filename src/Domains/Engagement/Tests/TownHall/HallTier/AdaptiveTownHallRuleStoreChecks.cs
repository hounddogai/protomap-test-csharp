namespace Acme.Workforce.Engagement.Tests.TownHall.HallTier;

using Acme.Workforce.Engagement.TownHall.HallTier;

/// <summary>Checks of hall tier code, run by the project's runner.</summary>
public static class AdaptiveTownHallRuleStoreChecks
{
    public static void MemoryAdaptiveTownHallRuleStoreFindsWhatItSaved()
    {
        var store = AdaptiveTownHallRuleStore.NewMemoryAdaptiveTownHallRuleStore();
        var item = AdaptiveTownHallRule.NewAdaptiveTownHallRule("code-965", "name-525");
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
        MemoryAdaptiveTownHallRuleStoreFindsWhatItSaved();
    }
}
