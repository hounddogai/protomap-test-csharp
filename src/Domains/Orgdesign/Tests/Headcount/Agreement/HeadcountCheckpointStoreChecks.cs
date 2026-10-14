namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountCheckpointStoreChecks
{
    public static void MemoryHeadcountCheckpointStoreFindsWhatItSaved()
    {
        var store = HeadcountCheckpointStore.NewMemoryHeadcountCheckpointStore();
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("code-459", "name-34");
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
        MemoryHeadcountCheckpointStoreFindsWhatItSaved();
    }
}
