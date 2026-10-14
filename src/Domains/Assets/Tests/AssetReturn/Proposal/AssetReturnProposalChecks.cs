namespace Acme.Workforce.Assets.Tests.AssetReturn.Proposal;

using Acme.Workforce.Assets.AssetReturn.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class AssetReturnProposalChecks
{
    public static void AssetReturnProposalStartsInProposed()
    {
        var item = AssetReturnProposal.NewAssetReturnProposal("code-137", "label-277");
        Check.Equal(item.Status, AssetReturnProposalStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AssetReturnProposalMovesToApproved()
    {
        var item = AssetReturnProposal.NewAssetReturnProposal("code-137", "label-277");
        try
        {
            item.MoveTo(AssetReturnProposalStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AssetReturnProposalStatus.Approved, "status");
    }

    public static void AssetReturnProposalRejectsMovesFromWithdrawn()
    {
        var item = AssetReturnProposal.NewAssetReturnProposal("code-137", "label-277");
        item.Status = AssetReturnProposalStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(AssetReturnProposalStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        AssetReturnProposalStartsInProposed();
        AssetReturnProposalMovesToApproved();
        AssetReturnProposalRejectsMovesFromWithdrawn();
    }
}
