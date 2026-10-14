namespace Acme.Workforce.Talent.Tests.Mobility.RecordItem;

using Acme.Workforce.Talent.Mobility.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class RollingMobilityProposalChecks
{
    public static void RollingMobilityProposalStartsInPlanned()
    {
        var item = RollingMobilityProposal.NewRollingMobilityProposal("code-120", "label-142");
        Check.Equal(item.Status, RollingMobilityProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingMobilityProposalMovesToCancelled()
    {
        var item = RollingMobilityProposal.NewRollingMobilityProposal("code-120", "label-142");
        try
        {
            item.MoveTo(RollingMobilityProposalStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingMobilityProposalStatus.Cancelled, "status");
    }

    public static void RollingMobilityProposalRejectsMovesFromCancelled()
    {
        var item = RollingMobilityProposal.NewRollingMobilityProposal("code-120", "label-142");
        item.Status = RollingMobilityProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RollingMobilityProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RollingMobilityProposalStartsInPlanned();
        RollingMobilityProposalMovesToCancelled();
        RollingMobilityProposalRejectsMovesFromCancelled();
    }
}
