namespace Acme.Workforce.Absence.Tests.Policy.Profile;

using Acme.Workforce.Absence.Policy.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class PolicyProfileStoreChecks
{
    public static void MemoryPolicyProfileStoreFindsWhatItSaved()
    {
        var store = PolicyProfileStore.NewMemoryPolicyProfileStore();
        var item = PolicyProfile.NewPolicyProfile("code-422", "label-156");
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
        MemoryPolicyProfileStoreFindsWhatItSaved();
    }
}
