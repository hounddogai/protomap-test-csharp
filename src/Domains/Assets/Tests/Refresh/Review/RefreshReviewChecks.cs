namespace Acme.Workforce.Assets.Tests.Refresh.Review;

using Acme.Workforce.Assets.Refresh.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RefreshReviewChecks
{
    public static void RefreshReviewStartsInPlanned()
    {
        var item = RefreshReview.NewRefreshReview("code-169", "title-231");
        Check.Equal(item.Status, RefreshReviewStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RefreshReviewMovesToScheduled()
    {
        var item = RefreshReview.NewRefreshReview("code-169", "title-231");
        try
        {
            item.MoveTo(RefreshReviewStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RefreshReviewStatus.Scheduled, "status");
    }

    public static void RefreshReviewRejectsMovesFromCancelled()
    {
        var item = RefreshReview.NewRefreshReview("code-169", "title-231");
        item.Status = RefreshReviewStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RefreshReviewStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RefreshReviewStartsInPlanned();
        RefreshReviewMovesToScheduled();
        RefreshReviewRejectsMovesFromCancelled();
    }
}
