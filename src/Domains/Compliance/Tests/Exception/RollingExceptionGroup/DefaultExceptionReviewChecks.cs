namespace Acme.Workforce.Compliance.Tests.Exception.RollingExceptionGroup;

using Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

/// <summary>Checks of rolling exception group code, run by the project's runner.</summary>
public static class DefaultExceptionReviewChecks
{
    public static void DefaultExceptionReviewStartsInNew()
    {
        var item = DefaultExceptionReview.NewDefaultExceptionReview("code-815", "label-780");
        Check.Equal(item.Status, DefaultExceptionReviewStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void DefaultExceptionReviewMovesToDone()
    {
        var item = DefaultExceptionReview.NewDefaultExceptionReview("code-815", "label-780");
        try
        {
            item.MoveTo(DefaultExceptionReviewStatus.Done);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, DefaultExceptionReviewStatus.Done, "status");
    }

    public static void DefaultExceptionReviewRejectsMovesFromFailed()
    {
        var item = DefaultExceptionReview.NewDefaultExceptionReview("code-815", "label-780");
        item.Status = DefaultExceptionReviewStatus.Failed;
        Check.Fails(() => item.MoveTo(DefaultExceptionReviewStatus.New), "move");
    }

    public static void RunAll()
    {
        DefaultExceptionReviewStartsInNew();
        DefaultExceptionReviewMovesToDone();
        DefaultExceptionReviewRejectsMovesFromFailed();
    }
}
