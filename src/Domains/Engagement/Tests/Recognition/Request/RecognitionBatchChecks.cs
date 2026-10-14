namespace Acme.Workforce.Engagement.Tests.Recognition.Request;

using Acme.Workforce.Engagement.Recognition.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class RecognitionBatchChecks
{
    public static void RecognitionBatchStartsInPlanned()
    {
        var item = RecognitionBatch.NewRecognitionBatch("code-837", "label-126");
        Check.Equal(item.Status, RecognitionBatchStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RecognitionBatchMovesToScheduled()
    {
        var item = RecognitionBatch.NewRecognitionBatch("code-837", "label-126");
        try
        {
            item.MoveTo(RecognitionBatchStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RecognitionBatchStatus.Scheduled, "status");
    }

    public static void RecognitionBatchRejectsMovesFromCancelled()
    {
        var item = RecognitionBatch.NewRecognitionBatch("code-837", "label-126");
        item.Status = RecognitionBatchStatus.Cancelled;
        Check.Fails(() => item.MoveTo(RecognitionBatchStatus.Planned), "move");
    }

    public static void RunAll()
    {
        RecognitionBatchStartsInPlanned();
        RecognitionBatchMovesToScheduled();
        RecognitionBatchRejectsMovesFromCancelled();
    }
}
