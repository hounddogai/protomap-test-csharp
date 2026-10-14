namespace Acme.Workforce.Compensation.Tests.BonusPool.Review;

using Acme.Workforce.Compensation.BonusPool.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class BonusPoolReviewChecks
{
    public static void BonusPoolReviewStartsInIdle()
    {
        var item = BonusPoolReview.NewBonusPoolReview("code-840", "title-115");
        Check.Equal(item.Status, BonusPoolReviewStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolReviewMovesToReady()
    {
        var item = BonusPoolReview.NewBonusPoolReview("code-840", "title-115");
        try
        {
            item.MoveTo(BonusPoolReviewStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolReviewStatus.Ready, "status");
    }

    public static void BonusPoolReviewRejectsMovesFromStopped()
    {
        var item = BonusPoolReview.NewBonusPoolReview("code-840", "title-115");
        item.Status = BonusPoolReviewStatus.Stopped;
        Check.Fails(() => item.MoveTo(BonusPoolReviewStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BonusPoolReviewStartsInIdle();
        BonusPoolReviewMovesToReady();
        BonusPoolReviewRejectsMovesFromStopped();
    }
}
