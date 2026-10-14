namespace Acme.Workforce.Compliance.Tests.Exception.ExceptionCheckpoint;

using Acme.Workforce.Compliance.Exception.ExceptionCheckpoint;

/// <summary>Checks of exception checkpoint code, run by the project's runner.</summary>
public static class CappedExceptionCheckpointStoreChecks
{
    public static void MemoryCappedExceptionCheckpointStoreFindsWhatItSaved()
    {
        var store = CappedExceptionCheckpointStore.NewMemoryCappedExceptionCheckpointStore();
        var item = CappedExceptionCheckpoint.NewCappedExceptionCheckpoint("code-909", "title-108");
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
        MemoryCappedExceptionCheckpointStoreFindsWhatItSaved();
    }
}
