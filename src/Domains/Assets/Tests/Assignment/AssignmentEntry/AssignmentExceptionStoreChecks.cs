namespace Acme.Workforce.Assets.Tests.Assignment.AssignmentEntry;

using Acme.Workforce.Assets.Assignment.AssignmentEntry;

/// <summary>Checks of assignment entry code, run by the project's runner.</summary>
public static class AssignmentExceptionStoreChecks
{
    public static void MemoryAssignmentExceptionStoreFindsWhatItSaved()
    {
        var store = AssignmentExceptionStore.NewMemoryAssignmentExceptionStore();
        var item = AssignmentException.NewAssignmentException("code-612", "title-79");
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
        MemoryAssignmentExceptionStoreFindsWhatItSaved();
    }
}
