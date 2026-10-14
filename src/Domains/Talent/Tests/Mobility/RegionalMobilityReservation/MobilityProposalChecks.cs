namespace Acme.Workforce.Talent.Tests.Mobility.RegionalMobilityReservation;

using Acme.Workforce.Talent.Mobility.RegionalMobilityReservation;

/// <summary>Checks of regional mobility reservation code, run by the project's runner.</summary>
public static class MobilityProposalChecks
{
    public static void MobilityProposalStartsInNew()
    {
        var item = MobilityProposal.NewMobilityProposal("code-96", "name-493");
        Check.Equal(item.Status, MobilityProposalStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MobilityProposalMovesToRunning()
    {
        var item = MobilityProposal.NewMobilityProposal("code-96", "name-493");
        try
        {
            item.MoveTo(MobilityProposalStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MobilityProposalStatus.Running, "status");
    }

    public static void MobilityProposalRejectsMovesFromFailed()
    {
        var item = MobilityProposal.NewMobilityProposal("code-96", "name-493");
        item.Status = MobilityProposalStatus.Failed;
        Check.Fails(() => item.MoveTo(MobilityProposalStatus.New), "move");
    }

    public static void RunAll()
    {
        MobilityProposalStartsInNew();
        MobilityProposalMovesToRunning();
        MobilityProposalRejectsMovesFromFailed();
    }
}
