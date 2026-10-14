namespace Acme.Workforce.Compliance.Tests.Exception.Profile;

using Acme.Workforce.Compliance.Exception.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class RegionalExceptionCheckpointStoreChecks
{
    public static void MemoryRegionalExceptionCheckpointStoreFindsWhatItSaved()
    {
        var store = RegionalExceptionCheckpointStore.NewMemoryRegionalExceptionCheckpointStore();
        var item = RegionalExceptionCheckpoint.NewRegionalExceptionCheckpoint("code-91", "title-487");
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
        MemoryRegionalExceptionCheckpointStoreFindsWhatItSaved();
    }
}
