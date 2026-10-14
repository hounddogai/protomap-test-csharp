namespace Acme.Workforce.Compliance.Tests.TrainingDue.DueWindow;

using Acme.Workforce.Compliance.TrainingDue.DueWindow;

/// <summary>Checks of due window code, run by the project's runner.</summary>
public static class AdaptiveTrainingDueExceptionStoreChecks
{
    public static void MemoryAdaptiveTrainingDueExceptionStoreFindsWhatItSaved()
    {
        var store = AdaptiveTrainingDueExceptionStore.NewMemoryAdaptiveTrainingDueExceptionStore();
        var item = AdaptiveTrainingDueException.NewAdaptiveTrainingDueException("code-245", "name-143");
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
        MemoryAdaptiveTrainingDueExceptionStoreFindsWhatItSaved();
    }
}
