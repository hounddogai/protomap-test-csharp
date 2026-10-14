namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PrimaryPositionProposalViewChecks
{
    public static void PrimaryPositionProposalViewShowsTheStatusInCapitals()
    {
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("code-819", "label-613");
        var view = PrimaryPositionProposalView.ToPrimaryPositionProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryPositionProposalView.ToPrimaryPositionProposalViews(new List<PrimaryPositionProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryPositionProposalViewShowsTheStatusInCapitals();
    }
}
