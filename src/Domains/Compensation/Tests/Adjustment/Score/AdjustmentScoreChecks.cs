namespace Acme.Workforce.Compensation.Tests.Adjustment.Score;

using Acme.Workforce.Compensation.Adjustment.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class AdjustmentScoreChecks
{
    public static void AdjustmentScoreStartsInOpen()
    {
        var item = AdjustmentScore.NewAdjustmentScore("code-284", "title-374");
        Check.Equal(item.Status, AdjustmentScoreStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentScoreMovesToInReview()
    {
        var item = AdjustmentScore.NewAdjustmentScore("code-284", "title-374");
        try
        {
            item.MoveTo(AdjustmentScoreStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentScoreStatus.InReview, "status");
    }

    public static void AdjustmentScoreRejectsMovesFromArchived()
    {
        var item = AdjustmentScore.NewAdjustmentScore("code-284", "title-374");
        item.Status = AdjustmentScoreStatus.Archived;
        Check.Fails(() => item.MoveTo(AdjustmentScoreStatus.Open), "move");
    }

    public static void RunAll()
    {
        AdjustmentScoreStartsInOpen();
        AdjustmentScoreMovesToInReview();
        AdjustmentScoreRejectsMovesFromArchived();
    }
}
