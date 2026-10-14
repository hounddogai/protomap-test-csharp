namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class FeedbackExceptionChecks
{
    public static void FeedbackExceptionStartsInRequested()
    {
        var item = FeedbackException.NewFeedbackException("code-106", "label-939");
        Check.Equal(item.Status, FeedbackExceptionStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void FeedbackExceptionMovesToGranted()
    {
        var item = FeedbackException.NewFeedbackException("code-106", "label-939");
        try
        {
            item.MoveTo(FeedbackExceptionStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, FeedbackExceptionStatus.Granted, "status");
    }

    public static void FeedbackExceptionRejectsMovesFromExpired()
    {
        var item = FeedbackException.NewFeedbackException("code-106", "label-939");
        item.Status = FeedbackExceptionStatus.Expired;
        Check.Fails(() => item.MoveTo(FeedbackExceptionStatus.Requested), "move");
    }

    public static void RunAll()
    {
        FeedbackExceptionStartsInRequested();
        FeedbackExceptionMovesToGranted();
        FeedbackExceptionRejectsMovesFromExpired();
    }
}
