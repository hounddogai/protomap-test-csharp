namespace Acme.Workforce.Compensation.Tests.Adjustment.Estimate;

using Acme.Workforce.Compensation.Adjustment.Estimate;

/// <summary>Checks of estimate code, run by the project's runner.</summary>
public static class SeasonalAdjustmentProposalChecks
{
    public static void SeasonalAdjustmentProposalStartsInPlanned()
    {
        var item = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal("code-769", "label-55");
        Check.Equal(item.Status, SeasonalAdjustmentProposalStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAdjustmentProposalMovesToScheduled()
    {
        var item = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal("code-769", "label-55");
        try
        {
            item.MoveTo(SeasonalAdjustmentProposalStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAdjustmentProposalStatus.Scheduled, "status");
    }

    public static void SeasonalAdjustmentProposalRejectsMovesFromCancelled()
    {
        var item = SeasonalAdjustmentProposal.NewSeasonalAdjustmentProposal("code-769", "label-55");
        item.Status = SeasonalAdjustmentProposalStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalAdjustmentProposalStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentProposalStartsInPlanned();
        SeasonalAdjustmentProposalMovesToScheduled();
        SeasonalAdjustmentProposalRejectsMovesFromCancelled();
    }
}
