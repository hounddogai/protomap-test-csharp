namespace Acme.Workforce.Talent.Tests.Mobility.Variance;

using Acme.Workforce.Talent.Mobility.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class AdaptiveMobilityProposalViewChecks
{
    public static void AdaptiveMobilityProposalViewShowsTheStatusInCapitals()
    {
        var item = AdaptiveMobilityProposal.NewAdaptiveMobilityProposal("code-202", "name-401");
        var view = AdaptiveMobilityProposalView.ToAdaptiveMobilityProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdaptiveMobilityProposalView.ToAdaptiveMobilityProposalViews(new List<AdaptiveMobilityProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdaptiveMobilityProposalViewShowsTheStatusInCapitals();
    }
}
