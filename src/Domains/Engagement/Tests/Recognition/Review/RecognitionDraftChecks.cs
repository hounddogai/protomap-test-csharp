namespace Acme.Workforce.Engagement.Tests.Recognition.Review;

using Acme.Workforce.Engagement.Recognition.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class RecognitionDraftChecks
{
    public static void RecognitionDraftStartsInNew()
    {
        var item = RecognitionDraft.NewRecognitionDraft("code-551", "name-745");
        Check.Equal(item.Status, RecognitionDraftStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionDraftMovesToFailed()
    {
        var item = RecognitionDraft.NewRecognitionDraft("code-551", "name-745");
        try
        {
            item.MoveTo(RecognitionDraftStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionDraftStatus.Failed, "status");
    }

    public static void RecognitionDraftRejectsMovesFromFailed()
    {
        var item = RecognitionDraft.NewRecognitionDraft("code-551", "name-745");
        item.Status = RecognitionDraftStatus.Failed;
        Check.Fails(() => item.MoveTo(RecognitionDraftStatus.New), "move");
    }

    public static void RunAll()
    {
        RecognitionDraftStartsInNew();
        RecognitionDraftMovesToFailed();
        RecognitionDraftRejectsMovesFromFailed();
    }
}
