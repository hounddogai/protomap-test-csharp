namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class RegionalAssetReturnProposalChecks
{
    public static void RegionalAssetReturnProposalStartsInDraft()
    {
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("code-936", "title-394");
        Check.Equal(item.Status, RegionalAssetReturnProposalStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RegionalAssetReturnProposalMovesToPending()
    {
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("code-936", "title-394");
        try
        {
            item.MoveTo(RegionalAssetReturnProposalStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RegionalAssetReturnProposalStatus.Pending, "status");
    }

    public static void RegionalAssetReturnProposalRejectsMovesFromClosed()
    {
        var item = RegionalAssetReturnProposal.NewRegionalAssetReturnProposal("code-936", "title-394");
        item.Status = RegionalAssetReturnProposalStatus.Closed;
        Check.Fails(() => item.MoveTo(RegionalAssetReturnProposalStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RegionalAssetReturnProposalStartsInDraft();
        RegionalAssetReturnProposalMovesToPending();
        RegionalAssetReturnProposalRejectsMovesFromClosed();
    }
}
