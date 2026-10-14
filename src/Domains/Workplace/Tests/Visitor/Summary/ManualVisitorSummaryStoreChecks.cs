namespace Acme.Workforce.Workplace.Tests.Visitor.Summary;

using Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ManualVisitorSummaryStoreChecks
{
    public static void MemoryManualVisitorSummaryStoreFindsWhatItSaved()
    {
        var store = ManualVisitorSummaryStore.NewMemoryManualVisitorSummaryStore();
        var item = ManualVisitorSummary.NewManualVisitorSummary("code-973", "name-694");
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
        MemoryManualVisitorSummaryStoreFindsWhatItSaved();
    }
}
