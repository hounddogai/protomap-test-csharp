namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class StrictPolicyAssignmentStoreChecks
{
    public static void MemoryStrictPolicyAssignmentStoreFindsWhatItSaved()
    {
        var store = StrictPolicyAssignmentStore.NewMemoryStrictPolicyAssignmentStore();
        var item = StrictPolicyAssignment.NewStrictPolicyAssignment("code-465", "label-471");
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
        MemoryStrictPolicyAssignmentStoreFindsWhatItSaved();
    }
}
