namespace Acme.Workforce.Absence.Tests.Policy.PolicyRule;

using Acme.Workforce.Absence.Policy.PolicyRule;

/// <summary>Checks of policy rule code, run by the project's runner.</summary>
public static class DefaultPolicyItemStoreChecks
{
    public static void MemoryDefaultPolicyItemStoreFindsWhatItSaved()
    {
        var store = DefaultPolicyItemStore.NewMemoryDefaultPolicyItemStore();
        var item = DefaultPolicyItem.NewDefaultPolicyItem("code-7", "name-823");
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
        MemoryDefaultPolicyItemStoreFindsWhatItSaved();
    }
}
