namespace Acme.Workforce.Assets.Tests.Assignment.Summary;

using Acme.Workforce.Assets.Assignment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class AssignmentSummaryStoreChecks
{
    public static void MemoryAssignmentSummaryStoreFindsWhatItSaved()
    {
        var store = AssignmentSummaryStore.NewMemoryAssignmentSummaryStore();
        var item = AssignmentSummary.NewAssignmentSummary("code-279", "name-198");
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
        MemoryAssignmentSummaryStoreFindsWhatItSaved();
    }
}
