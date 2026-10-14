namespace Acme.Workforce.Orgdesign.Tests.Position.PositionThreshold;

using Acme.Workforce.Orgdesign.Position.PositionThreshold;

/// <summary>Checks of position threshold code, run by the project's runner.</summary>
public static class PrimaryPositionProposalChecks
{
    public static void PrimaryPositionProposalStartsInDraft()
    {
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("code-819", "label-613");
        Check.Equal(item.Status, PrimaryPositionProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryPositionProposalMovesToSuspended()
    {
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("code-819", "label-613");
        try
        {
            item.MoveTo(PrimaryPositionProposalStatus.Suspended);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryPositionProposalStatus.Suspended, "status");
    }

    public static void PrimaryPositionProposalRejectsMovesFromClosed()
    {
        var item = PrimaryPositionProposal.NewPrimaryPositionProposal("code-819", "label-613");
        item.Status = PrimaryPositionProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(PrimaryPositionProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        PrimaryPositionProposalStartsInDraft();
        PrimaryPositionProposalMovesToSuspended();
        PrimaryPositionProposalRejectsMovesFromClosed();
    }
}
