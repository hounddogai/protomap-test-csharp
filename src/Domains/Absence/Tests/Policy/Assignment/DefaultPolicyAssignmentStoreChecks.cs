namespace Acme.Workforce.Absence.Tests.Policy.Assignment;

using Acme.Workforce.Absence.Policy.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class DefaultPolicyAssignmentStoreChecks
{
    public static void MemoryDefaultPolicyAssignmentStoreFindsWhatItSaved()
    {
        var store = DefaultPolicyAssignmentStore.NewMemoryDefaultPolicyAssignmentStore();
        var item = DefaultPolicyAssignment.NewDefaultPolicyAssignment("code-296", "name-825");
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
        MemoryDefaultPolicyAssignmentStoreFindsWhatItSaved();
    }
}
