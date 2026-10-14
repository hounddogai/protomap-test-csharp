namespace Acme.Workforce.Assets.Tests.Assignment.Agreement;

using Acme.Workforce.Assets.Assignment.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class AssignmentPolicyStoreChecks
{
    public static void MemoryAssignmentPolicyStoreFindsWhatItSaved()
    {
        var store = AssignmentPolicyStore.NewMemoryAssignmentPolicyStore();
        var item = AssignmentPolicy.NewAssignmentPolicy("code-151", "title-788");
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
        MemoryAssignmentPolicyStoreFindsWhatItSaved();
    }
}
