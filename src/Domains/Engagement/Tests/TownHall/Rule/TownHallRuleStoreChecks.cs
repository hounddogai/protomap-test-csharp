namespace Acme.Workforce.Engagement.Tests.TownHall.Rule;

using Acme.Workforce.Engagement.TownHall.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class TownHallRuleStoreChecks
{
    public static void MemoryTownHallRuleStoreFindsWhatItSaved()
    {
        var store = TownHallRuleStore.NewMemoryTownHallRuleStore();
        var item = TownHallRule.NewTownHallRule("code-75", "name-47");
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
        MemoryTownHallRuleStoreFindsWhatItSaved();
    }
}
