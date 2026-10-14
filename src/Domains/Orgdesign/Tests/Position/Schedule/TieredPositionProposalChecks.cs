namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredPositionProposalChecks
{
    public static void TieredPositionProposalStartsInCreated()
    {
        var item = TieredPositionProposal.NewTieredPositionProposal("code-556", "label-274");
        Check.Equal(item.Status, TieredPositionProposalStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TieredPositionProposalMovesToValidated()
    {
        var item = TieredPositionProposal.NewTieredPositionProposal("code-556", "label-274");
        try
        {
            item.MoveTo(TieredPositionProposalStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TieredPositionProposalStatus.Validated, "status");
    }

    public static void TieredPositionProposalRejectsMovesFromRetired()
    {
        var item = TieredPositionProposal.NewTieredPositionProposal("code-556", "label-274");
        item.Status = TieredPositionProposalStatus.Retired;
        Check.Fails(() => item.MoveTo(TieredPositionProposalStatus.Created), "move");
    }

    public static void RunAll()
    {
        TieredPositionProposalStartsInCreated();
        TieredPositionProposalMovesToValidated();
        TieredPositionProposalRejectsMovesFromRetired();
    }
}
