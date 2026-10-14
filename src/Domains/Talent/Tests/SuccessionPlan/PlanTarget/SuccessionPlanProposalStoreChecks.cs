namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanTarget;

using Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

/// <summary>Checks of plan target code, run by the project's runner.</summary>
public static class SuccessionPlanProposalStoreChecks
{
    public static void MemorySuccessionPlanProposalStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanProposalStore.NewMemorySuccessionPlanProposalStore();
        var item = SuccessionPlanProposal.NewSuccessionPlanProposal("code-566", "name-505");
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
        MemorySuccessionPlanProposalStoreFindsWhatItSaved();
    }
}
