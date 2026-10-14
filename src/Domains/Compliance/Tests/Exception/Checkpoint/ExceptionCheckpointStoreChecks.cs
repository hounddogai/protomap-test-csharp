namespace Acme.Workforce.Compliance.Tests.Exception.Checkpoint;

using Acme.Workforce.Compliance.Exception.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class ExceptionCheckpointStoreChecks
{
    public static void MemoryExceptionCheckpointStoreFindsWhatItSaved()
    {
        var store = ExceptionCheckpointStore.NewMemoryExceptionCheckpointStore();
        var item = ExceptionCheckpoint.NewExceptionCheckpoint("code-328", "label-568");
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
        MemoryExceptionCheckpointStoreFindsWhatItSaved();
    }
}
