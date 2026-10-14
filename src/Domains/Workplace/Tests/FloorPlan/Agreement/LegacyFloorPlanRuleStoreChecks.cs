namespace Acme.Workforce.Workplace.Tests.FloorPlan.Agreement;

using Acme.Workforce.Workplace.FloorPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class LegacyFloorPlanRuleStoreChecks
{
    public static void MemoryLegacyFloorPlanRuleStoreFindsWhatItSaved()
    {
        var store = LegacyFloorPlanRuleStore.NewMemoryLegacyFloorPlanRuleStore();
        var item = LegacyFloorPlanRule.NewLegacyFloorPlanRule("code-234", "label-3");
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
        MemoryLegacyFloorPlanRuleStoreFindsWhatItSaved();
    }
}
