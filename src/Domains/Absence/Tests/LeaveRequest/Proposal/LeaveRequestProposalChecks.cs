namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LeaveRequestProposalChecks
{
    public static void LeaveRequestProposalStartsInCreated()
    {
        var item = LeaveRequestProposal.NewLeaveRequestProposal("code-538", "label-147");
        Check.Equal(item.Status, LeaveRequestProposalStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestProposalMovesToRetired()
    {
        var item = LeaveRequestProposal.NewLeaveRequestProposal("code-538", "label-147");
        try
        {
            item.MoveTo(LeaveRequestProposalStatus.Retired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestProposalStatus.Retired, "status");
    }

    public static void LeaveRequestProposalRejectsMovesFromRetired()
    {
        var item = LeaveRequestProposal.NewLeaveRequestProposal("code-538", "label-147");
        item.Status = LeaveRequestProposalStatus.Retired;
        Check.Fails(() => item.MoveTo(LeaveRequestProposalStatus.Created), "move");
    }

    public static void RunAll()
    {
        LeaveRequestProposalStartsInCreated();
        LeaveRequestProposalMovesToRetired();
        LeaveRequestProposalRejectsMovesFromRetired();
    }
}
