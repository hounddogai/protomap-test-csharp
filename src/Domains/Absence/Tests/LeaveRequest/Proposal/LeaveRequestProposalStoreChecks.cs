namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LeaveRequestProposalStoreChecks
{
    public static void MemoryLeaveRequestProposalStoreFindsWhatItSaved()
    {
        var store = LeaveRequestProposalStore.NewMemoryLeaveRequestProposalStore();
        var item = LeaveRequestProposal.NewLeaveRequestProposal("code-538", "label-147");
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
        MemoryLeaveRequestProposalStoreFindsWhatItSaved();
    }
}
