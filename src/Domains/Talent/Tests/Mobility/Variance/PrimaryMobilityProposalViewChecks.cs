namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class PrimaryMobilityProposalViewChecks
{
    public static void PrimaryMobilityProposalViewShowsTheStatusInCapitals()
    {
        var item = PrimaryMobilityProposal.NewPrimaryMobilityProposal("code-902", "name-684");
        var view = PrimaryMobilityProposalView.ToPrimaryMobilityProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryMobilityProposalView.ToPrimaryMobilityProposalViews(new List<PrimaryMobilityProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryMobilityProposalViewShowsTheStatusInCapitals();
    }
}
