namespace Acme.Workforce.Contractors.Tests.Rate.Review;

using Acme.Workforce.Contractors.Rate.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RateReviewChecks
{
    public static void RateReviewStartsInIdle()
    {
        var item = RateReview.NewRateReview("code-112", "name-982");
        Check.Equal(item.Status, RateReviewStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RateReviewMovesToWarming()
    {
        var item = RateReview.NewRateReview("code-112", "name-982");
        try
        {
            item.MoveTo(RateReviewStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RateReviewStatus.Warming, "status");
    }

    public static void RateReviewRejectsMovesFromStopped()
    {
        var item = RateReview.NewRateReview("code-112", "name-982");
        item.Status = RateReviewStatus.Stopped;
        Check.Fails(() => item.MoveTo(RateReviewStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RateReviewStartsInIdle();
        RateReviewMovesToWarming();
        RateReviewRejectsMovesFromStopped();
    }
}
