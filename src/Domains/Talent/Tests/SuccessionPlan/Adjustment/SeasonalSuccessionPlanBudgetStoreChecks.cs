namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Adjustment;

using Acme.Workforce.Talent.SuccessionPlan.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SeasonalSuccessionPlanBudgetStoreChecks
{
    public static void MemorySeasonalSuccessionPlanBudgetStoreFindsWhatItSaved()
    {
        var store = SeasonalSuccessionPlanBudgetStore.NewMemorySeasonalSuccessionPlanBudgetStore();
        var item = SeasonalSuccessionPlanBudget.NewSeasonalSuccessionPlanBudget("code-355", "label-953");
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
        MemorySeasonalSuccessionPlanBudgetStoreFindsWhatItSaved();
    }
}
