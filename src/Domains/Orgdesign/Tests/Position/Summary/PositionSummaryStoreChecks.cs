namespace Acme.Workforce.Orgdesign.Tests.Position.Summary;

using Acme.Workforce.Orgdesign.Position.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PositionSummaryStoreChecks
{
    public static void MemoryPositionSummaryStoreFindsWhatItSaved()
    {
        var store = PositionSummaryStore.NewMemoryPositionSummaryStore();
        var item = PositionSummary.NewPositionSummary("code-92", "title-447");
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
        MemoryPositionSummaryStoreFindsWhatItSaved();
    }
}
