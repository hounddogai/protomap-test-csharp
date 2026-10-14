namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueGroupStoreChecks
{
    public static void MemoryTrainingDueGroupStoreFindsWhatItSaved()
    {
        var store = TrainingDueGroupStore.NewMemoryTrainingDueGroupStore();
        var item = TrainingDueGroup.NewTrainingDueGroup("code-784", "title-37");
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
        MemoryTrainingDueGroupStoreFindsWhatItSaved();
    }
}
