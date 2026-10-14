namespace Acme.Workforce.Absence.Tests.LeaveRequest.Proposal;

using Acme.Workforce.Absence.LeaveRequest.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class LeaveRequestProposalViewChecks
{
    public static void LeaveRequestProposalViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestProposal.NewLeaveRequestProposal("code-538", "label-147");
        var view = LeaveRequestProposalView.ToLeaveRequestProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestProposalView.ToLeaveRequestProposalViews(new List<LeaveRequestProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestProposalViewShowsTheStatusInCapitals();
    }
}
