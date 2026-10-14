namespace Acme.Workforce.Workplace.Tests.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.Visitor.VisitorEntry;

/// <summary>Checks of visitor entry code, run by the project's runner.</summary>
public static class PendingVisitorAssignmentStoreChecks
{
    public static void MemoryPendingVisitorAssignmentStoreFindsWhatItSaved()
    {
        var store = PendingVisitorAssignmentStore.NewMemoryPendingVisitorAssignmentStore();
        var item = PendingVisitorAssignment.NewPendingVisitorAssignment("code-185", "label-951");
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
        MemoryPendingVisitorAssignmentStoreFindsWhatItSaved();
    }
}
