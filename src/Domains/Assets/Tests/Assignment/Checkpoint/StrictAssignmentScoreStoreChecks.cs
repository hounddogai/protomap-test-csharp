namespace Acme.Workforce.Assets.Tests.Assignment.Checkpoint;

using Acme.Workforce.Assets.Assignment.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class StrictAssignmentScoreStoreChecks
{
    public static void MemoryStrictAssignmentScoreStoreFindsWhatItSaved()
    {
        var store = StrictAssignmentScoreStore.NewMemoryStrictAssignmentScoreStore();
        var item = StrictAssignmentScore.NewStrictAssignmentScore("code-461", "name-530");
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
        MemoryStrictAssignmentScoreStoreFindsWhatItSaved();
    }
}
