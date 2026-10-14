namespace Acme.Workforce.Compliance.Tests.TrainingDue.Variance;

using Acme.Workforce.Compliance.TrainingDue.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class TrainingDueExceptionStoreChecks
{
    public static void MemoryTrainingDueExceptionStoreFindsWhatItSaved()
    {
        var store = TrainingDueExceptionStore.NewMemoryTrainingDueExceptionStore();
        var item = TrainingDueException.NewTrainingDueException("code-327", "title-464");
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
        MemoryTrainingDueExceptionStoreFindsWhatItSaved();
    }
}
