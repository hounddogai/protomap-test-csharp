namespace Acme.Workforce.Engagement.Tests.TownHall.BatchedHallRule;

using Acme.Workforce.Engagement.TownHall.BatchedHallRule;

/// <summary>Checks of batched hall rule code, run by the project's runner.</summary>
public static class PrimaryTownHallRuleStoreChecks
{
    public static void MemoryPrimaryTownHallRuleStoreFindsWhatItSaved()
    {
        var store = PrimaryTownHallRuleStore.NewMemoryPrimaryTownHallRuleStore();
        var item = PrimaryTownHallRule.NewPrimaryTownHallRule("code-94", "title-996");
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
        MemoryPrimaryTownHallRuleStoreFindsWhatItSaved();
    }
}
