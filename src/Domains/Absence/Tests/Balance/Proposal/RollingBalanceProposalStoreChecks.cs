namespace Acme.Workforce.Absence.Tests.Balance.Proposal;

using Acme.Workforce.Absence.Balance.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RollingBalanceProposalStoreChecks
{
    public static void MemoryRollingBalanceProposalStoreFindsWhatItSaved()
    {
        var store = RollingBalanceProposalStore.NewMemoryRollingBalanceProposalStore();
        var item = RollingBalanceProposal.NewRollingBalanceProposal("code-741", "label-939");
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
        MemoryRollingBalanceProposalStoreFindsWhatItSaved();
    }
}
