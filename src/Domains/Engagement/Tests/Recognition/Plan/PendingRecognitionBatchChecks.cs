namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingRecognitionBatchChecks
{
    public static void PendingRecognitionBatchStartsInRequested()
    {
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("code-835", "title-732");
        Check.Equal(item.Status, PendingRecognitionBatchStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingRecognitionBatchMovesToGranted()
    {
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("code-835", "title-732");
        try
        {
            item.MoveTo(PendingRecognitionBatchStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingRecognitionBatchStatus.Granted, "status");
    }

    public static void PendingRecognitionBatchRejectsMovesFromExpired()
    {
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("code-835", "title-732");
        item.Status = PendingRecognitionBatchStatus.Expired;
        Check.Fails(() => item.MoveTo(PendingRecognitionBatchStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PendingRecognitionBatchStartsInRequested();
        PendingRecognitionBatchMovesToGranted();
        PendingRecognitionBatchRejectsMovesFromExpired();
    }
}
