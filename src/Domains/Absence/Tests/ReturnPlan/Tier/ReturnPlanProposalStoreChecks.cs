namespace Acme.Workforce.Absence.Tests.ReturnPlan.Tier;

using Acme.Workforce.Absence.ReturnPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class ReturnPlanProposalStoreChecks
{
    public static void MemoryReturnPlanProposalStoreFindsWhatItSaved()
    {
        var store = ReturnPlanProposalStore.NewMemoryReturnPlanProposalStore();
        var item = ReturnPlanProposal.NewReturnPlanProposal("code-734", "label-371");
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
        MemoryReturnPlanProposalStoreFindsWhatItSaved();
    }
}
