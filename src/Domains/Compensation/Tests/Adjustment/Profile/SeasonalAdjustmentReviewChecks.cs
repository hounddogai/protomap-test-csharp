namespace Acme.Workforce.Compensation.Tests.Adjustment.Profile;

using Acme.Workforce.Compensation.Adjustment.Profile;

/// <summary>Checks of profile code, run by the project's runner.</summary>
public static class SeasonalAdjustmentReviewChecks
{
    public static void SeasonalAdjustmentReviewStartsInProposed()
    {
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("code-600", "title-42");
        Check.Equal(item.Status, SeasonalAdjustmentReviewStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAdjustmentReviewMovesToRejected()
    {
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("code-600", "title-42");
        try
        {
            item.MoveTo(SeasonalAdjustmentReviewStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAdjustmentReviewStatus.Rejected, "status");
    }

    public static void SeasonalAdjustmentReviewRejectsMovesFromWithdrawn()
    {
        var item = SeasonalAdjustmentReview.NewSeasonalAdjustmentReview("code-600", "title-42");
        item.Status = SeasonalAdjustmentReviewStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalAdjustmentReviewStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentReviewStartsInProposed();
        SeasonalAdjustmentReviewMovesToRejected();
        SeasonalAdjustmentReviewRejectsMovesFromWithdrawn();
    }
}
