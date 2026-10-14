namespace Acme.Workforce.Compliance.Tests.Exception.Assignment;

using Acme.Workforce.Compliance.Exception.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class ExceptionAssignmentStoreChecks
{
    public static void MemoryExceptionAssignmentStoreFindsWhatItSaved()
    {
        var store = ExceptionAssignmentStore.NewMemoryExceptionAssignmentStore();
        var item = ExceptionAssignment.NewExceptionAssignment("code-347", "title-47");
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
        MemoryExceptionAssignmentStoreFindsWhatItSaved();
    }
}
