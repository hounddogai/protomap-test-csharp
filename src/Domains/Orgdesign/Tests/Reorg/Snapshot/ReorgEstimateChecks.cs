namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ReorgEstimateChecks
{
    public static void ReorgEstimateStartsInOpen()
    {
        var item = ReorgEstimate.NewReorgEstimate("code-145", "label-26");
        Check.Equal(item.Status, ReorgEstimateStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgEstimateMovesToInReview()
    {
        var item = ReorgEstimate.NewReorgEstimate("code-145", "label-26");
        try
        {
            item.MoveTo(ReorgEstimateStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgEstimateStatus.InReview, "status");
    }

    public static void ReorgEstimateRejectsMovesFromArchived()
    {
        var item = ReorgEstimate.NewReorgEstimate("code-145", "label-26");
        item.Status = ReorgEstimateStatus.Archived;
        Check.Fails(() => item.MoveTo(ReorgEstimateStatus.Open), "move");
    }

    public static void RunAll()
    {
        ReorgEstimateStartsInOpen();
        ReorgEstimateMovesToInReview();
        ReorgEstimateRejectsMovesFromArchived();
    }
}
