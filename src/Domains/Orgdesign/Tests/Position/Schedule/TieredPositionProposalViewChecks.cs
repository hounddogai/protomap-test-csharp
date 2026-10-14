namespace Acme.Workforce.Orgdesign.Tests.Position.Schedule;

using Acme.Workforce.Orgdesign.Position.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class TieredPositionProposalViewChecks
{
    public static void TieredPositionProposalViewShowsTheStatusInCapitals()
    {
        var item = TieredPositionProposal.NewTieredPositionProposal("code-556", "label-274");
        var view = TieredPositionProposalView.ToTieredPositionProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredPositionProposalView.ToTieredPositionProposalViews(new List<TieredPositionProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredPositionProposalViewShowsTheStatusInCapitals();
    }
}
