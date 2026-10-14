namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Item;

using Acme.Workforce.Talent.SuccessionPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanRuleStoreChecks
{
    public static void MemorySeasonalSuccessionPlanRuleStoreFindsWhatItSaved()
    {
        var store = SeasonalSuccessionPlanRuleStore.NewMemorySeasonalSuccessionPlanRuleStore();
        var item = SeasonalSuccessionPlanRule.NewSeasonalSuccessionPlanRule("code-48", "title-21");
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
        MemorySeasonalSuccessionPlanRuleStoreFindsWhatItSaved();
    }
}
