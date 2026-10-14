namespace Acme.Workforce.Workplace.Tests.Occupancy.Plan;

using Acme.Workforce.Workplace.Occupancy.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class StrictOccupancyReviewChecks
{
    public static void StrictOccupancyReviewStartsInCreated()
    {
        var item = StrictOccupancyReview.NewStrictOccupancyReview("code-869", "title-229");
        Check.Equal(item.Status, StrictOccupancyReviewStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictOccupancyReviewMovesToPublished()
    {
        var item = StrictOccupancyReview.NewStrictOccupancyReview("code-869", "title-229");
        try
        {
            item.MoveTo(StrictOccupancyReviewStatus.Published);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictOccupancyReviewStatus.Published, "status");
    }

    public static void StrictOccupancyReviewRejectsMovesFromRetired()
    {
        var item = StrictOccupancyReview.NewStrictOccupancyReview("code-869", "title-229");
        item.Status = StrictOccupancyReviewStatus.Retired;
        Check.Fails(() => item.MoveTo(StrictOccupancyReviewStatus.Created), "move");
    }

    public static void RunAll()
    {
        StrictOccupancyReviewStartsInCreated();
        StrictOccupancyReviewMovesToPublished();
        StrictOccupancyReviewRejectsMovesFromRetired();
    }
}
