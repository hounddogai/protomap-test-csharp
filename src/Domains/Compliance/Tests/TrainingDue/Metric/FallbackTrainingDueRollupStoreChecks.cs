namespace Acme.Workforce.Compliance.Tests.TrainingDue.Metric;

using Acme.Workforce.Compliance.TrainingDue.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackTrainingDueRollupStoreChecks
{
    public static void MemoryFallbackTrainingDueRollupStoreFindsWhatItSaved()
    {
        var store = FallbackTrainingDueRollupStore.NewMemoryFallbackTrainingDueRollupStore();
        var item = FallbackTrainingDueRollup.NewFallbackTrainingDueRollup("code-850", "title-688");
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
        MemoryFallbackTrainingDueRollupStoreFindsWhatItSaved();
    }
}
