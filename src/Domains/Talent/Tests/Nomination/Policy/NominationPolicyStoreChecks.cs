namespace Acme.Workforce.Talent.Tests.Nomination.Policy;

using Acme.Workforce.Talent.Nomination.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class NominationPolicyStoreChecks
{
    public static void MemoryNominationPolicyStoreFindsWhatItSaved()
    {
        var store = NominationPolicyStore.NewMemoryNominationPolicyStore();
        var item = NominationPolicy.NewNominationPolicy("code-334", "label-801");
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
        MemoryNominationPolicyStoreFindsWhatItSaved();
    }
}
