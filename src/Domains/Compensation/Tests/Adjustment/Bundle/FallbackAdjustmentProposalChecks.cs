namespace Acme.Workforce.Compensation.Tests.Adjustment.Bundle;

using Acme.Workforce.Compensation.Adjustment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackAdjustmentProposalChecks
{
    public static void FallbackAdjustmentProposalStartsInPlanned()
    {
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("code-487", "label-265");
        Check.Equal(item.Status, FallbackAdjustmentProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackAdjustmentProposalMovesToCompleted()
    {
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("code-487", "label-265");
        try
        {
            item.MoveTo(FallbackAdjustmentProposalStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackAdjustmentProposalStatus.Completed, "status");
    }

    public static void FallbackAdjustmentProposalRejectsMovesFromCancelled()
    {
        var item = FallbackAdjustmentProposal.NewFallbackAdjustmentProposal("code-487", "label-265");
        item.Status = FallbackAdjustmentProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(FallbackAdjustmentProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        FallbackAdjustmentProposalStartsInPlanned();
        FallbackAdjustmentProposalMovesToCompleted();
        FallbackAdjustmentProposalRejectsMovesFromCancelled();
    }
}
