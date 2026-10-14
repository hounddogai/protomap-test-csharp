namespace Acme.Workforce.Engagement.Tests.Recognition.Segment;

using Acme.Workforce.Engagement.Recognition.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class ManualRecognitionSegmentChecks
{
    public static void ManualRecognitionSegmentStartsInDraft()
    {
        var item = ManualRecognitionSegment.NewManualRecognitionSegment("code-70", "title-462");
        Check.Equal(item.Status, ManualRecognitionSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ManualRecognitionSegmentMovesToPending()
    {
        var item = ManualRecognitionSegment.NewManualRecognitionSegment("code-70", "title-462");
        try
        {
            item.MoveTo(ManualRecognitionSegmentStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ManualRecognitionSegmentStatus.Pending, "status");
    }

    public static void ManualRecognitionSegmentRejectsMovesFromClosed()
    {
        var item = ManualRecognitionSegment.NewManualRecognitionSegment("code-70", "title-462");
        item.Status = ManualRecognitionSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(ManualRecognitionSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        ManualRecognitionSegmentStartsInDraft();
        ManualRecognitionSegmentMovesToPending();
        ManualRecognitionSegmentRejectsMovesFromClosed();
    }
}
