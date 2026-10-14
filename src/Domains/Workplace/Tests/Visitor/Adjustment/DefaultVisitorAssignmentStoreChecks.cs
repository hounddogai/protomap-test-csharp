namespace Acme.Workforce.Workplace.Tests.Visitor.Adjustment;

using Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class DefaultVisitorAssignmentStoreChecks
{
    public static void MemoryDefaultVisitorAssignmentStoreFindsWhatItSaved()
    {
        var store = DefaultVisitorAssignmentStore.NewMemoryDefaultVisitorAssignmentStore();
        var item = DefaultVisitorAssignment.NewDefaultVisitorAssignment("code-51", "label-180");
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
        MemoryDefaultVisitorAssignmentStoreFindsWhatItSaved();
    }
}
