namespace Acme.Workforce.Compliance.Tests.TrainingDue.CappedDueVariance;

using Acme.Workforce.Compliance.TrainingDue.CappedDueVariance;

/// <summary>Checks of capped due variance code, run by the project's runner.</summary>
public static class PendingTrainingDueBatchStoreChecks
{
    public static void MemoryPendingTrainingDueBatchStoreFindsWhatItSaved()
    {
        var store = PendingTrainingDueBatchStore.NewMemoryPendingTrainingDueBatchStore();
        var item = PendingTrainingDueBatch.NewPendingTrainingDueBatch("code-478", "label-600");
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
        MemoryPendingTrainingDueBatchStoreFindsWhatItSaved();
    }
}
