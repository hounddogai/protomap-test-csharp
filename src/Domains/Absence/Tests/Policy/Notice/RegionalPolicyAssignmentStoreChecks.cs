namespace Acme.Workforce.Absence.Tests.Policy.Notice;

using Acme.Workforce.Absence.Policy.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RegionalPolicyAssignmentStoreChecks
{
    public static void MemoryRegionalPolicyAssignmentStoreFindsWhatItSaved()
    {
        var store = RegionalPolicyAssignmentStore.NewMemoryRegionalPolicyAssignmentStore();
        var item = RegionalPolicyAssignment.NewRegionalPolicyAssignment("code-279", "title-497");
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
        MemoryRegionalPolicyAssignmentStoreFindsWhatItSaved();
    }
}
