namespace Acme.Workforce.Compensation.Tests.Adjustment.Rule;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class AdjustmentProposalChecks
{
    public static void AdjustmentProposalStartsInPlanned()
    {
        var item = AdjustmentProposal.NewAdjustmentProposal("code-369", "name-463");
        Check.Equal(item.Status, AdjustmentProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentProposalMovesToCancelled()
    {
        var item = AdjustmentProposal.NewAdjustmentProposal("code-369", "name-463");
        try
        {
            item.MoveTo(AdjustmentProposalStatus.Cancelled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentProposalStatus.Cancelled, "status");
    }

    public static void AdjustmentProposalRejectsMovesFromCancelled()
    {
        var item = AdjustmentProposal.NewAdjustmentProposal("code-369", "name-463");
        item.Status = AdjustmentProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AdjustmentProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AdjustmentProposalStartsInPlanned();
        AdjustmentProposalMovesToCancelled();
        AdjustmentProposalRejectsMovesFromCancelled();
    }
}
