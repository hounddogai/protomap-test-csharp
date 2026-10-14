namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleProposalStoreChecks
{
    public static void MemoryMeritCycleProposalStoreFindsWhatItSaved()
    {
        var store = MeritCycleProposalStore.NewMemoryMeritCycleProposalStore();
        var item = MeritCycleProposal.NewMeritCycleProposal("code-480", "name-816");
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
        MemoryMeritCycleProposalStoreFindsWhatItSaved();
    }
}
