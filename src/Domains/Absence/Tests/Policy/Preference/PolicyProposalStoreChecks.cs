namespace Acme.Workforce.Absence.Tests.Policy.Preference;

using Acme.Workforce.Absence.Policy.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class PolicyProposalStoreChecks
{
    public static void MemoryPolicyProposalStoreFindsWhatItSaved()
    {
        var store = PolicyProposalStore.NewMemoryPolicyProposalStore();
        var item = PolicyProposal.NewPolicyProposal("code-626", "name-613");
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
        MemoryPolicyProposalStoreFindsWhatItSaved();
    }
}
