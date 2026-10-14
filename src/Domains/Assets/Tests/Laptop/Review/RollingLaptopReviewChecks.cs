namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RollingLaptopReviewChecks
{
    public static void RollingLaptopReviewStartsInNew()
    {
        var item = RollingLaptopReview.NewRollingLaptopReview("code-566", "label-618");
        Check.Equal(item.Status, RollingLaptopReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingLaptopReviewMovesToDone()
    {
        var item = RollingLaptopReview.NewRollingLaptopReview("code-566", "label-618");
        try
        {
            item.MoveTo(RollingLaptopReviewStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingLaptopReviewStatus.Done, "status");
    }

    public static void RollingLaptopReviewRejectsMovesFromFailed()
    {
        var item = RollingLaptopReview.NewRollingLaptopReview("code-566", "label-618");
        item.Status = RollingLaptopReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(RollingLaptopReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        RollingLaptopReviewStartsInNew();
        RollingLaptopReviewMovesToDone();
        RollingLaptopReviewRejectsMovesFromFailed();
    }
}
