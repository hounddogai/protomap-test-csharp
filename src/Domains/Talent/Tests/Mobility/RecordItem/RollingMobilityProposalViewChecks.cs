namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RollingMobilityProposalViewChecks
{
    public static void RollingMobilityProposalViewShowsTheStatusInCapitals()
    {
        var item = RollingMobilityProposal.NewRollingMobilityProposal("code-120", "label-142");
        var view = RollingMobilityProposalView.ToRollingMobilityProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingMobilityProposalView.ToRollingMobilityProposalViews(new List<RollingMobilityProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingMobilityProposalViewShowsTheStatusInCapitals();
    }
}
