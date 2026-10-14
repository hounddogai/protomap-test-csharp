namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackAdjustmentProposalViewChecks
{
    public static void FallbackAdjustmentProposalViewShowsTheStatusInCapitals()
    {
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("code-487", "label-265");
        var view = FallbackAdjustmentProposalView.ToFallbackAdjustmentProposalView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackAdjustmentProposalView.ToFallbackAdjustmentProposalViews(new List<FallbackAdjustmentProposal> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackAdjustmentProposalViewShowsTheStatusInCapitals();
    }
}
