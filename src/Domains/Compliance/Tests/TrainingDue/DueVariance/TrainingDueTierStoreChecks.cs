namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueVariance;

using Acme.Workforce.Compliance.TrainingDue.DueVariance;

/// <summary>Checks of due variance code, run by the project's runner.</summary>
public static class TrainingDueTierStoreChecks
{
    public static void MemoryTrainingDueTierStoreFindsWhatItSaved()
    {
        var store = TrainingDueTierStore.NewMemoryTrainingDueTierStore();
        var item = TrainingDueTier.NewTrainingDueTier("code-697", "label-128");
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
        MemoryTrainingDueTierStoreFindsWhatItSaved();
    }
}
