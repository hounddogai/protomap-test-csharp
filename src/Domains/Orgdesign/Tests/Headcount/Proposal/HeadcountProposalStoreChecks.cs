namespace Acme.Workforce.Orgdesign.Tests.Headcount.Proposal;

using Acme.Workforce.Orgdesign.Headcount.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class HeadcountProposalStoreChecks
{
    public static void MemoryHeadcountProposalStoreFindsWhatItSaved()
    {
        var store = HeadcountProposalStore.NewMemoryHeadcountProposalStore();
        var item = HeadcountProposal.NewHeadcountProposal("code-146", "title-693");
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
        MemoryHeadcountProposalStoreFindsWhatItSaved();
    }
}
