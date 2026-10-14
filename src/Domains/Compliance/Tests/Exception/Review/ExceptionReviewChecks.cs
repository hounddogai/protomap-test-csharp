namespace Acme.Workforce.Compliance.Tests.Exception.Review;

using Acme.Workforce.Compliance.Exception.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class ExceptionReviewChecks
{
    public static void ExceptionReviewStartsInRequested()
    {
        var item = ExceptionReview.NewExceptionReview("code-441", "name-735");
        Check.Equal(item.Status, ExceptionReviewStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ExceptionReviewMovesToGranted()
    {
        var item = ExceptionReview.NewExceptionReview("code-441", "name-735");
        try
        {
            item.MoveTo(ExceptionReviewStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ExceptionReviewStatus.Granted, "status");
    }

    public static void ExceptionReviewRejectsMovesFromExpired()
    {
        var item = ExceptionReview.NewExceptionReview("code-441", "name-735");
        item.Status = ExceptionReviewStatus.Expired;
        Check.Fails(() => item.MoveTo(ExceptionReviewStatus.Requested), "move");
    }

    public static void RunAll()
    {
        ExceptionReviewStartsInRequested();
        ExceptionReviewMovesToGranted();
        ExceptionReviewRejectsMovesFromExpired();
    }
}
