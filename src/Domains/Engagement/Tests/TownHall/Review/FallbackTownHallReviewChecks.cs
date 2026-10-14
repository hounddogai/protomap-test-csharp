namespace Acme.Workforce.Engagement.Tests.TownHall.Review;

using Acme.Workforce.Engagement.TownHall.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class FallbackTownHallReviewChecks
{
    public static void FallbackTownHallReviewStartsInNew()
    {
        var item = FallbackTownHallReview.NewFallbackTownHallReview("code-596", "label-1");
        Check.Equal(item.Status, FallbackTownHallReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FallbackTownHallReviewMovesToQueued()
    {
        var item = FallbackTownHallReview.NewFallbackTownHallReview("code-596", "label-1");
        try
        {
            item.MoveTo(FallbackTownHallReviewStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FallbackTownHallReviewStatus.Queued, "status");
    }

    public static void FallbackTownHallReviewRejectsMovesFromFailed()
    {
        var item = FallbackTownHallReview.NewFallbackTownHallReview("code-596", "label-1");
        item.Status = FallbackTownHallReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(FallbackTownHallReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        FallbackTownHallReviewStartsInNew();
        FallbackTownHallReviewMovesToQueued();
        FallbackTownHallReviewRejectsMovesFromFailed();
    }
}
