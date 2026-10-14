namespace Acme.Workforce.Engagement.Tests.Recognition.Digest;

using Acme.Workforce.Engagement.Recognition.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class RecognitionSegmentChecks
{
    public static void RecognitionSegmentStartsInIdle()
    {
        var item = RecognitionSegment.NewRecognitionSegment("code-530", "title-102");
        Check.Equal(item.Status, RecognitionSegmentStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionSegmentMovesToStopped()
    {
        var item = RecognitionSegment.NewRecognitionSegment("code-530", "title-102");
        try
        {
            item.MoveTo(RecognitionSegmentStatus.Stopped);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionSegmentStatus.Stopped, "status");
    }

    public static void RecognitionSegmentRejectsMovesFromStopped()
    {
        var item = RecognitionSegment.NewRecognitionSegment("code-530", "title-102");
        item.Status = RecognitionSegmentStatus.Stopped;
        Check.Fails(() => item.MoveTo(RecognitionSegmentStatus.Idle), "move");
    }

    public static void RunAll()
    {
        RecognitionSegmentStartsInIdle();
        RecognitionSegmentMovesToStopped();
        RecognitionSegmentRejectsMovesFromStopped();
    }
}
