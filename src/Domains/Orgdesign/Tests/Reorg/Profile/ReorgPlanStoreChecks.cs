namespace Acme.Workforce.Orgdesign.Tests.Reorg.Profile;

using Acme.Workforce.Orgdesign.Reorg.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class ReorgPlanStoreChecks
{
    public static void MemoryReorgPlanStoreFindsWhatItSaved()
    {
        var store = ReorgPlanStore.NewMemoryReorgPlanStore();
        var item = ReorgPlan.NewReorgPlan("code-321", "label-454");
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
        MemoryReorgPlanStoreFindsWhatItSaved();
    }
}
