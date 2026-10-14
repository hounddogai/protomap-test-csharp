namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class OccupancyProposalChecks
{
    public static void OccupancyProposalStartsInDraft()
    {
        var item = OccupancyProposal.NewOccupancyProposal("code-661", "label-205");
        Check.Equal(item.Status, OccupancyProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void OccupancyProposalMovesToSuspended()
    {
        var item = OccupancyProposal.NewOccupancyProposal("code-661", "label-205");
        try
        {
            item.MoveTo(OccupancyProposalStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, OccupancyProposalStatus.Suspended, "status");
    }

    public static void OccupancyProposalRejectsMovesFromClosed()
    {
        var item = OccupancyProposal.NewOccupancyProposal("code-661", "label-205");
        item.Status = OccupancyProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(OccupancyProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        OccupancyProposalStartsInDraft();
        OccupancyProposalMovesToSuspended();
        OccupancyProposalRejectsMovesFromClosed();
    }
}
