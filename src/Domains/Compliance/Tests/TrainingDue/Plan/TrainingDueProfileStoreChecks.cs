namespace Acme.Workforce.Compliance.Tests.TrainingDue.Plan;

using Acme.Workforce.Compliance.TrainingDue.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class TrainingDueProfileStoreChecks
{
    public static void MemoryTrainingDueProfileStoreFindsWhatItSaved()
    {
        var store = TrainingDueProfileStore.NewMemoryTrainingDueProfileStore();
        var item = TrainingDueProfile.NewTrainingDueProfile("code-900", "label-500");
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
        MemoryTrainingDueProfileStoreFindsWhatItSaved();
    }
}
