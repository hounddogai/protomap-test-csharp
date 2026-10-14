namespace Acme.Workforce.Assets.Tests.AssetReturn.Summary;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ScheduledAssetReturnProposalViewChecks
{
    public static void ScheduledAssetReturnProposalViewShowsTheStatusInCapitals()
    {
        var item = ScheduledAssetReturnProposal.NewScheduledAssetReturnProposal("code-747", "name-492");
        var view = ScheduledAssetReturnProposalView.ToScheduledAssetReturnProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledAssetReturnProposalView.ToScheduledAssetReturnProposalViews(new List<ScheduledAssetReturnProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledAssetReturnProposalViewShowsTheStatusInCapitals();
    }
}
