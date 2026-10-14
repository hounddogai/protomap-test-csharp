namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FeedbackVarianceChecks
{
    public static void FeedbackVarianceStartsInNew()
    {
        var item = FeedbackVariance.NewFeedbackVariance("code-711", "title-123");
        Check.Equal(item.Status, FeedbackVarianceStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackVarianceMovesToDone()
    {
        var item = FeedbackVariance.NewFeedbackVariance("code-711", "title-123");
        try
        {
            item.MoveTo(FeedbackVarianceStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackVarianceStatus.Done, "status");
    }

    public static void FeedbackVarianceRejectsMovesFromFailed()
    {
        var item = FeedbackVariance.NewFeedbackVariance("code-711", "title-123");
        item.Status = FeedbackVarianceStatus.Failed;
        Check.Fails(() => item.MoveTo(FeedbackVarianceStatus.New), "move");
    }

    public static void RunAll()
    {
        FeedbackVarianceStartsInNew();
        FeedbackVarianceMovesToDone();
        FeedbackVarianceRejectsMovesFromFailed();
    }
}
