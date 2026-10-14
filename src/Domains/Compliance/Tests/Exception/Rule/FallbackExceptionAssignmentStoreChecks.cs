namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FallbackExceptionAssignmentStoreChecks
{
    public static void MemoryFallbackExceptionAssignmentStoreFindsWhatItSaved()
    {
        var store = FallbackExceptionAssignmentStore.NewMemoryFallbackExceptionAssignmentStore();
        var item = FallbackExceptionAssignment.NewFallbackExceptionAssignment("code-45", "name-352");
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
        MemoryFallbackExceptionAssignmentStoreFindsWhatItSaved();
    }
}
