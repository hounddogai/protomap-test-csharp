namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgPlan;

using Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

/// <summary>Checks of reorg plan code, run by the project's runner.</summary>
public static class WeightedReorgPlanStoreChecks
{
    public static void MemoryWeightedReorgPlanStoreFindsWhatItSaved()
    {
        var store = WeightedReorgPlanStore.NewMemoryWeightedReorgPlanStore();
        var item = WeightedReorgPlan.NewWeightedReorgPlan("code-400", "title-443");
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
        MemoryWeightedReorgPlanStoreFindsWhatItSaved();
    }
}
