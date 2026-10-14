namespace Acme.Workforce.Absence.Tests.Policy.Score;

using Acme.Workforce.Absence.Policy.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class PolicyAssignmentStoreChecks
{
    public static void MemoryPolicyAssignmentStoreFindsWhatItSaved()
    {
        var store = PolicyAssignmentStore.NewMemoryPolicyAssignmentStore();
        var item = PolicyAssignment.NewPolicyAssignment("code-258", "title-612");
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
        MemoryPolicyAssignmentStoreFindsWhatItSaved();
    }
}
