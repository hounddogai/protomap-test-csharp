namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class CappedVisitorAssignmentStoreChecks
{
    public static void MemoryCappedVisitorAssignmentStoreFindsWhatItSaved()
    {
        var store = CappedVisitorAssignmentStore.NewMemoryCappedVisitorAssignmentStore();
        var item = CappedVisitorAssignment.NewCappedVisitorAssignment("code-72", "title-204");
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
        MemoryCappedVisitorAssignmentStoreFindsWhatItSaved();
    }
}
