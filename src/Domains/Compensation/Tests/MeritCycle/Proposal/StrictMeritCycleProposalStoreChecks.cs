namespace Acme.Workforce.Compensation.Tests.MeritCycle.Proposal;

using Acme.Workforce.Compensation.MeritCycle.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class StrictMeritCycleProposalStoreChecks
{
    public static void MemoryStrictMeritCycleProposalStoreFindsWhatItSaved()
    {
        var store = StrictMeritCycleProposalStore.NewMemoryStrictMeritCycleProposalStore();
        var item = StrictMeritCycleProposal.NewStrictMeritCycleProposal("code-434", "name-96");
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
        MemoryStrictMeritCycleProposalStoreFindsWhatItSaved();
    }
}
