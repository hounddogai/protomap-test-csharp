namespace Acme.Workforce.Absence.Tests.Policy.Cycle;

using Acme.Workforce.Absence.Policy.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class PolicyBundleStoreChecks
{
    public static void MemoryPolicyBundleStoreFindsWhatItSaved()
    {
        var store = PolicyBundleStore.NewMemoryPolicyBundleStore();
        var item = PolicyBundle.NewPolicyBundle("code-832", "label-514");
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
        MemoryPolicyBundleStoreFindsWhatItSaved();
    }
}
