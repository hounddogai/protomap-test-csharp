namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentProposalViewChecks
{
    public static void AdjustmentProposalViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentProposal.NewAdjustmentProposal("code-369", "name-463");
        var view = AdjustmentProposalView.ToAdjustmentProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentProposalView.ToAdjustmentProposalViews(new List<AdjustmentProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentProposalViewShowsTheStatusInCapitals();
    }
}
