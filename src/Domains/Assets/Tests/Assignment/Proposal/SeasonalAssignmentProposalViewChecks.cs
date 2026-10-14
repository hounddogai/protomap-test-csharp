namespace Acme.Workforce.Assets.Tests.Assignment.Proposal;

using Acme.Workforce.Assets.Assignment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SeasonalAssignmentProposalViewChecks
{
    public static void SeasonalAssignmentProposalViewShowsTheStatusInCapitals()
    {
        var item = SeasonalAssignmentProposal.NewSeasonalAssignmentProposal("code-808", "name-108");
        var view = SeasonalAssignmentProposalView.ToSeasonalAssignmentProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalAssignmentProposalView.ToSeasonalAssignmentProposalViews(new List<SeasonalAssignmentProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalAssignmentProposalViewShowsTheStatusInCapitals();
    }
}
