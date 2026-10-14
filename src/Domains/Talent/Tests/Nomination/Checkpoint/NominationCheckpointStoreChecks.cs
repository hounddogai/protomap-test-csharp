namespace Acme.Workforce.Talent.Tests.Nomination.Checkpoint;

using Acme.Workforce.Talent.Nomination.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class NominationCheckpointStoreChecks
{
    public static void MemoryNominationCheckpointStoreFindsWhatItSaved()
    {
        var store = NominationCheckpointStore.NewMemoryNominationCheckpointStore();
        var item = NominationCheckpoint.NewNominationCheckpoint("code-198", "title-553");
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
        MemoryNominationCheckpointStoreFindsWhatItSaved();
    }
}
