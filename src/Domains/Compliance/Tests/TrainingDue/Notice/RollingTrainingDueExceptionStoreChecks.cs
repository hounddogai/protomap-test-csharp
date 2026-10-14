namespace Acme.Workforce.Compliance.Tests.TrainingDue.Notice;

using Acme.Workforce.Compliance.TrainingDue.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingTrainingDueExceptionStoreChecks
{
    public static void MemoryRollingTrainingDueExceptionStoreFindsWhatItSaved()
    {
        var store = RollingTrainingDueExceptionStore.NewMemoryRollingTrainingDueExceptionStore();
        var item = RollingTrainingDueException.NewRollingTrainingDueException("code-273", "name-751");
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
        MemoryRollingTrainingDueExceptionStoreFindsWhatItSaved();
    }
}
