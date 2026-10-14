namespace Acme.Workforce.Engagement.Tests.Recognition.Digest;

using Acme.Workforce.Engagement.Recognition.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RecognitionDigestChecks
{
    public static void RecognitionDigestStartsInOpen()
    {
        var item = RecognitionDigest.NewRecognitionDigest("code-235", "title-504");
        Check.Equal(item.Status, RecognitionDigestStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionDigestMovesToReopened()
    {
        var item = RecognitionDigest.NewRecognitionDigest("code-235", "title-504");
        try
        {
            item.MoveTo(RecognitionDigestStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionDigestStatus.Reopened, "status");
    }

    public static void RecognitionDigestRejectsMovesFromArchived()
    {
        var item = RecognitionDigest.NewRecognitionDigest("code-235", "title-504");
        item.Status = RecognitionDigestStatus.Archived;
        Check.Fails(() => item.MoveTo(RecognitionDigestStatus.Open), "move");
    }

    public static void RunAll()
    {
        RecognitionDigestStartsInOpen();
        RecognitionDigestMovesToReopened();
        RecognitionDigestRejectsMovesFromArchived();
    }
}
