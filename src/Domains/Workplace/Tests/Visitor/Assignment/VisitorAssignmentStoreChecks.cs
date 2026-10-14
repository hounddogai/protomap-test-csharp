namespace Acme.Workforce.Workplace.Tests.Visitor.Assignment;

using Acme.Workforce.Workplace.Visitor.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class VisitorAssignmentStoreChecks
{
    public static void MemoryVisitorAssignmentStoreFindsWhatItSaved()
    {
        var store = VisitorAssignmentStore.NewMemoryVisitorAssignmentStore();
        var item = VisitorAssignment.NewVisitorAssignment("code-608", "title-504");
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
        MemoryVisitorAssignmentStoreFindsWhatItSaved();
    }
}
