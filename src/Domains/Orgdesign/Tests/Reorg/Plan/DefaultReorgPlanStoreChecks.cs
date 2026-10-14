namespace Acme.Workforce.Orgdesign.Tests.Reorg.Plan;

using Acme.Workforce.Orgdesign.Reorg.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class DefaultReorgPlanStoreChecks
{
    public static void MemoryDefaultReorgPlanStoreFindsWhatItSaved()
    {
        var store = DefaultReorgPlanStore.NewMemoryDefaultReorgPlanStore();
        var item = DefaultReorgPlan.NewDefaultReorgPlan("code-358", "title-158");
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
        MemoryDefaultReorgPlanStoreFindsWhatItSaved();
    }
}
