namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictLeaveRequestReviewChecks
{
    public static void StrictLeaveRequestReviewStartsInIdle()
    {
        var item = StrictLeaveRequestReview.NewStrictLeaveRequestReview("code-718", "title-546");
        Check.Equal(item.Status, StrictLeaveRequestReviewStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void StrictLeaveRequestReviewMovesToDraining()
    {
        var item = StrictLeaveRequestReview.NewStrictLeaveRequestReview("code-718", "title-546");
        try
        {
            item.MoveTo(StrictLeaveRequestReviewStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, StrictLeaveRequestReviewStatus.Draining, "status");
    }

    public static void StrictLeaveRequestReviewRejectsMovesFromStopped()
    {
        var item = StrictLeaveRequestReview.NewStrictLeaveRequestReview("code-718", "title-546");
        item.Status = StrictLeaveRequestReviewStatus.Stopped;
        Check.Fails(() => item.MoveTo(StrictLeaveRequestReviewStatus.Idle), "move");
    }

    public static void RunAll()
    {
        StrictLeaveRequestReviewStartsInIdle();
        StrictLeaveRequestReviewMovesToDraining();
        StrictLeaveRequestReviewRejectsMovesFromStopped();
    }
}
