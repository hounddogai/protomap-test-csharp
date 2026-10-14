namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class OccupancyProposalViewChecks
{
    public static void OccupancyProposalViewShowsTheStatusInCapitals()
    {
        var item = OccupancyProposal.NewOccupancyProposal("code-661", "label-205");
        var view = OccupancyProposalView.ToOccupancyProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(OccupancyProposalView.ToOccupancyProposalViews(new List<OccupancyProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        OccupancyProposalViewShowsTheStatusInCapitals();
    }
}
