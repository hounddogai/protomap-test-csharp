namespace Acme.Workforce.Compensation.Tests.Adjustment.Estimate;

using Acme.Workforce.Compensation.Adjustment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalAdjustmentProposalViewChecks
{
    public static void SeasonalAdjustmentProposalViewShowsTheStatusInCapitals()
    {
        var item = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal("code-769", "label-55");
        var view = SeasonalAdjustmentProposalView.ToSeasonalAdjustmentProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(SeasonalAdjustmentProposalView.ToSeasonalAdjustmentProposalViews(new List<SeasonalAdjustmentProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentProposalViewShowsTheStatusInCapitals();
    }
}
