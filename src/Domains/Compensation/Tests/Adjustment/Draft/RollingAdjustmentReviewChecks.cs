namespace Acme.Workforce.Compensation.Tests.Adjustment.Draft;

using Acme.Workforce.Compensation.Adjustment.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class RollingAdjustmentReviewChecks
{
    public static void RollingAdjustmentReviewStartsInNew()
    {
        var item = RollingAdjustmentReview.NewRollingAdjustmentReview("code-55", "label-594");
        Check.Equal(item.Status, RollingAdjustmentReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingAdjustmentReviewMovesToQueued()
    {
        var item = RollingAdjustmentReview.NewRollingAdjustmentReview("code-55", "label-594");
        try
        {
            item.MoveTo(RollingAdjustmentReviewStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingAdjustmentReviewStatus.Queued, "status");
    }

    public static void RollingAdjustmentReviewRejectsMovesFromFailed()
    {
        var item = RollingAdjustmentReview.NewRollingAdjustmentReview("code-55", "label-594");
        item.Status = RollingAdjustmentReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingAdjustmentReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingAdjustmentReviewStartsInNew();
        RollingAdjustmentReviewMovesToQueued();
        RollingAdjustmentReviewRejectsMovesFromFailed();
    }
}
