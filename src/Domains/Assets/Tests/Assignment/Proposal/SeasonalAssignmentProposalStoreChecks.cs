namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalAssignmentProposalStoreChecks
{
    public static void MemorySeasonalAssignmentProposalStoreFindsWhatItSaved()
    {
        var store = SeasonalAssignmentProposalStore.NewMemorySeasonalAssignmentProposalStore();
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("code-808", "name-108");
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
        MemorySeasonalAssignmentProposalStoreFindsWhatItSaved();
    }
}
