namespace Acme.Workforce.Engagement.Tests.Recognition.OverrideItem;

using Acme.Workforce.Engagement.Recognition.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class RecognitionReviewChecks
{
    public static void RecognitionReviewStartsInDraft()
    {
        var item = RecognitionReview.NewRecognitionReview("code-230", "title-310");
        Check.Equal(item.Status, RecognitionReviewStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionReviewMovesToPending()
    {
        var item = RecognitionReview.NewRecognitionReview("code-230", "title-310");
        try
        {
            item.MoveTo(RecognitionReviewStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionReviewStatus.Pending, "status");
    }

    public static void RecognitionReviewRejectsMovesFromClosed()
    {
        var item = RecognitionReview.NewRecognitionReview("code-230", "title-310");
        item.Status = RecognitionReviewStatus.Closed;
        Check.Fails(() => item.MoveTo(RecognitionReviewStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RecognitionReviewStartsInDraft();
        RecognitionReviewMovesToPending();
        RecognitionReviewRejectsMovesFromClosed();
    }
}
