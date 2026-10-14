namespace Acme.Workforce.Engagement.Tests.Recognition.Draft;

using Acme.Workforce.Engagement.Recognition.Draft;

/// <summary>Checks of draft code, run by the project's runner.</summary>
public static class BatchedRecognitionDraftChecks
{
    public static void BatchedRecognitionDraftStartsInPlanned()
    {
        var item = BatchedRecognitionDraft.NewBatchedRecognitionDraft("code-802", "name-202");
        Check.Equal(item.Status, BatchedRecognitionDraftStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedRecognitionDraftMovesToScheduled()
    {
        var item = BatchedRecognitionDraft.NewBatchedRecognitionDraft("code-802", "name-202");
        try
        {
            item.MoveTo(BatchedRecognitionDraftStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedRecognitionDraftStatus.Scheduled, "status");
    }

    public static void BatchedRecognitionDraftRejectsMovesFromCancelled()
    {
        var item = BatchedRecognitionDraft.NewBatchedRecognitionDraft("code-802", "name-202");
        item.Status = BatchedRecognitionDraftStatus.Cancelled;
        Check.Fails(() => item.MoveTo(BatchedRecognitionDraftStatus.Planned), "move");
    }

    public static void RunAll()
    {
        BatchedRecognitionDraftStartsInPlanned();
        BatchedRecognitionDraftMovesToScheduled();
        BatchedRecognitionDraftRejectsMovesFromCancelled();
    }
}
