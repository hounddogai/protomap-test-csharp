namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScheduledAssetReturnProposalChecks
{
    public static void ScheduledAssetReturnProposalStartsInRequested()
    {
        var item = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal("code-747", "name-492");
        Check.Equal(item.Status, ScheduledAssetReturnProposalStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledAssetReturnProposalMovesToExpired()
    {
        var item = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal("code-747", "name-492");
        try
        {
            item.MoveTo(ScheduledAssetReturnProposalStatus.Expired);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledAssetReturnProposalStatus.Expired, "status");
    }

    public static void ScheduledAssetReturnProposalRejectsMovesFromExpired()
    {
        var item = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal("code-747", "name-492");
        item.Status = ScheduledAssetReturnProposalStatus.Expired;
        Check.Fails(() => item.MoveTo(ScheduledAssetReturnProposalStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ScheduledAssetReturnProposalStartsInRequested();
        ScheduledAssetReturnProposalMovesToExpired();
        ScheduledAssetReturnProposalRejectsMovesFromExpired();
    }
}
