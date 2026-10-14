namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class LegacyTrainingDueVarianceStoreChecks
{
    public static void MemoryLegacyTrainingDueVarianceStoreFindsWhatItSaved()
    {
        var store = LegacyTrainingDueVarianceStore.NewMemoryLegacyTrainingDueVarianceStore();
        var item = LegacyTrainingDueVariance.NewLegacyTrainingDueVariance("code-870", "label-513");
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
        MemoryLegacyTrainingDueVarianceStoreFindsWhatItSaved();
    }
}
