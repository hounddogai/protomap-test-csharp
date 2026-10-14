namespace Acme.Workforce.Workplace.Tests.Visitor.Score;

using Acme.Workforce.Workplace.Visitor.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class SeasonalVisitorCheckpointStoreChecks
{
    public static void MemorySeasonalVisitorCheckpointStoreFindsWhatItSaved()
    {
        var store = SeasonalVisitorCheckpointStore.NewMemorySeasonalVisitorCheckpointStore();
        var item = SeasonalVisitorCheckpoint.NewSeasonalVisitorCheckpoint("code-809", "title-56");
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
        MemorySeasonalVisitorCheckpointStoreFindsWhatItSaved();
    }
}
