namespace Acme.Workforce.Compliance.Tests.TrainingDue.Baseline;

using Acme.Workforce.Compliance.TrainingDue.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class AdaptiveTrainingDuePlanStoreChecks
{
    public static void MemoryAdaptiveTrainingDuePlanStoreFindsWhatItSaved()
    {
        var store = AdaptiveTrainingDuePlanStore.NewMemoryAdaptiveTrainingDuePlanStore();
        var item = AdaptiveTrainingDuePlan.NewAdaptiveTrainingDuePlan("code-987", "label-708");
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
        MemoryAdaptiveTrainingDuePlanStoreFindsWhatItSaved();
    }
}
