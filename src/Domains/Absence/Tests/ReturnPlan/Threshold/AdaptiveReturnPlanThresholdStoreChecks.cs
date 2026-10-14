namespace Acme.Workforce.Absence.Tests.ReturnPlan.Threshold;

using Acme.Workforce.Absence.ReturnPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveReturnPlanThresholdStoreChecks
{
    public static void MemoryAdaptiveReturnPlanThresholdStoreFindsWhatItSaved()
    {
        var store = AdaptiveReturnPlanThresholdStore.NewMemoryAdaptiveReturnPlanThresholdStore();
        var item = AdaptiveReturnPlanThreshold.NewAdaptiveReturnPlanThreshold("code-509", "label-991");
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
        MemoryAdaptiveReturnPlanThresholdStoreFindsWhatItSaved();
    }
}
