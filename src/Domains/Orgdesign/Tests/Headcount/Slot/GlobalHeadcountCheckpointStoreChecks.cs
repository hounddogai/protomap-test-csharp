namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class GlobalHeadcountCheckpointStoreChecks
{
    public static void MemoryGlobalHeadcountCheckpointStoreFindsWhatItSaved()
    {
        var store = GlobalHeadcountCheckpointStore.NewMemoryGlobalHeadcountCheckpointStore();
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("code-385", "title-243");
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
        MemoryGlobalHeadcountCheckpointStoreFindsWhatItSaved();
    }
}
