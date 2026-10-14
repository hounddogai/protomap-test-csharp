namespace Acme.Workforce.Absence.Tests.Policy.OverrideItem;

using Acme.Workforce.Absence.Policy.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class PolicyOverrideStoreChecks
{
    public static void MemoryPolicyOverrideStoreFindsWhatItSaved()
    {
        var store = PolicyOverrideStore.NewMemoryPolicyOverrideStore();
        var item = PolicyOverride.NewPolicyOverride("code-521", "name-254");
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
        MemoryPolicyOverrideStoreFindsWhatItSaved();
    }
}
