namespace Acme.Workforce.Workplace.Tests.Visitor.Review;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class AdaptiveVisitorCheckpointStoreChecks
{
    public static void MemoryAdaptiveVisitorCheckpointStoreFindsWhatItSaved()
    {
        var store = AdaptiveVisitorCheckpointStore.NewMemoryAdaptiveVisitorCheckpointStore();
        var item = AdaptiveVisitorCheckpoint.NewAdaptiveVisitorCheckpoint("code-853", "title-34");
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
        MemoryAdaptiveVisitorCheckpointStoreFindsWhatItSaved();
    }
}
