namespace Acme.Workforce.Compliance.Tests.TrainingDue.Budget;

using Acme.Workforce.Compliance.TrainingDue.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class LegacyTrainingDueBatchStoreChecks
{
    public static void MemoryLegacyTrainingDueBatchStoreFindsWhatItSaved()
    {
        var store = LegacyTrainingDueBatchStore.NewMemoryLegacyTrainingDueBatchStore();
        var item = LegacyTrainingDueBatch.NewLegacyTrainingDueBatch("code-872", "name-92");
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
        MemoryLegacyTrainingDueBatchStoreFindsWhatItSaved();
    }
}
