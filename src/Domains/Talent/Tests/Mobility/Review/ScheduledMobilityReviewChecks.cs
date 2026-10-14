namespace Acme.Workforce.Talent.Tests.Mobility.Review;

using Acme.Workforce.Talent.Mobility.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ScheduledMobilityReviewChecks
{
    public static void ScheduledMobilityReviewStartsInNew()
    {
        var item = ScheduledMobilityReview.NewScheduledMobilityReview("code-415", "title-547");
        Check.Equal(item.Status, ScheduledMobilityReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ScheduledMobilityReviewMovesToRunning()
    {
        var item = ScheduledMobilityReview.NewScheduledMobilityReview("code-415", "title-547");
        try
        {
            item.MoveTo(ScheduledMobilityReviewStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ScheduledMobilityReviewStatus.Running, "status");
    }

    public static void ScheduledMobilityReviewRejectsMovesFromFailed()
    {
        var item = ScheduledMobilityReview.NewScheduledMobilityReview("code-415", "title-547");
        item.Status = ScheduledMobilityReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(ScheduledMobilityReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        ScheduledMobilityReviewStartsInNew();
        ScheduledMobilityReviewMovesToRunning();
        ScheduledMobilityReviewRejectsMovesFromFailed();
    }
}
