namespace Acme.Workforce.Absence.Tests.ReturnPlan.Agreement;

using Acme.Workforce.Absence.ReturnPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class TieredReturnPlanThresholdStoreChecks
{
    public static void MemoryTieredReturnPlanThresholdStoreFindsWhatItSaved()
    {
        var store = TieredReturnPlanThresholdStore.NewMemoryTieredReturnPlanThresholdStore();
        var item = TieredReturnPlanThreshold.NewTieredReturnPlanThreshold("code-560", "label-678");
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
        MemoryTieredReturnPlanThresholdStoreFindsWhatItSaved();
    }
}
