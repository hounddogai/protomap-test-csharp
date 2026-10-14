namespace Acme.Workforce.Engagement.Tests.Recognition.Variance;

using Acme.Workforce.Engagement.Recognition.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class RecognitionCheckpointChecks
{
    public static void RecognitionCheckpointStartsInDraft()
    {
        var item = RecognitionCheckpoint.NewRecognitionCheckpoint("code-157", "title-508");
        Check.Equal(item.Status, RecognitionCheckpointStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionCheckpointMovesToPending()
    {
        var item = RecognitionCheckpoint.NewRecognitionCheckpoint("code-157", "title-508");
        try
        {
            item.MoveTo(RecognitionCheckpointStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionCheckpointStatus.Pending, "status");
    }

    public static void RecognitionCheckpointRejectsMovesFromClosed()
    {
        var item = RecognitionCheckpoint.NewRecognitionCheckpoint("code-157", "title-508");
        item.Status = RecognitionCheckpointStatus.Closed;
        Check.Fails(() => item.MoveTo(RecognitionCheckpointStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RecognitionCheckpointStartsInDraft();
        RecognitionCheckpointMovesToPending();
        RecognitionCheckpointRejectsMovesFromClosed();
    }
}
