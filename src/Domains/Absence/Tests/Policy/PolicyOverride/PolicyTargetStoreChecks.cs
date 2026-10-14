namespace Acme.Workforce.Absence.Tests.Policy.PolicyOverride;

using Acme.Workforce.Absence.Policy.PolicyOverride;

/// <summary>Checks of policy override code, run by the project's runner.</summary>
public static class PolicyTargetStoreChecks
{
    public static void MemoryPolicyTargetStoreFindsWhatItSaved()
    {
        var store = PolicyTargetStore.NewMemoryPolicyTargetStore();
        var item = PolicyTarget.NewPolicyTarget("code-635", "title-538");
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
        MemoryPolicyTargetStoreFindsWhatItSaved();
    }
}
