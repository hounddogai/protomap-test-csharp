namespace Acme.Workforce.Compliance.Tests.TrainingDue.Batch;

using Acme.Workforce.Compliance.TrainingDue.Batch;

/// <summary>Checks of batch code, run by the project's runner.</summary>
public static class TrainingDueAllocationStoreChecks
{
    public static void MemoryTrainingDueAllocationStoreFindsWhatItSaved()
    {
        var store = TrainingDueAllocationStore.NewMemoryTrainingDueAllocationStore();
        var item = TrainingDueAllocation.NewTrainingDueAllocation("code-959", "title-610");
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
        MemoryTrainingDueAllocationStoreFindsWhatItSaved();
    }
}
