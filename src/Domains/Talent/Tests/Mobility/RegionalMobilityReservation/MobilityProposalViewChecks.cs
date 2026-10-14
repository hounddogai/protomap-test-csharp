namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class MobilityProposalViewChecks
{
    public static void MobilityProposalViewShowsTheStatusInCapitals()
    {
        var item = MobilityProposal.NewMobilityProposal("code-96", "name-493");
        var view = MobilityProposalView.ToMobilityProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(MobilityProposalView.ToMobilityProposalViews(new List<MobilityProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        MobilityProposalViewShowsTheStatusInCapitals();
    }
}
