namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class TieredReturnPlanRuleStoreChecks
{
    public static void MemoryTieredReturnPlanRuleStoreFindsWhatItSaved()
    {
        var store = TieredReturnPlanRuleStore.NewMemoryTieredReturnPlanRuleStore();
        var item = TieredReturnPlanRule.NewTieredReturnPlanRule("code-985", "name-451");
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
        MemoryTieredReturnPlanRuleStoreFindsWhatItSaved();
    }
}
