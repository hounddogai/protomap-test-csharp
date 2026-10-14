namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class WeightedTrainingDueProfileStoreChecks
{
    public static void MemoryWeightedTrainingDueProfileStoreFindsWhatItSaved()
    {
        var store = WeightedTrainingDueProfileStore.NewMemoryWeightedTrainingDueProfileStore();
        var item = WeightedTrainingDueProfile.NewWeightedTrainingDueProfile("code-83", "title-448");
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
        MemoryWeightedTrainingDueProfileStoreFindsWhatItSaved();
    }
}
