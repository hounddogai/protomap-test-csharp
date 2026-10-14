namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class PrimaryNominationPolicyStoreChecks
{
    public static void MemoryPrimaryNominationPolicyStoreFindsWhatItSaved()
    {
        var store = PrimaryNominationPolicyStore.NewMemoryPrimaryNominationPolicyStore();
        var item = PrimaryNominationPolicy.NewPrimaryNominationPolicy("code-319", "title-30");
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
        MemoryPrimaryNominationPolicyStoreFindsWhatItSaved();
    }
}
