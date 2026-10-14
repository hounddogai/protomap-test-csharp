namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedRecognitionSlotChecks
{
    public static void BatchedRecognitionSlotStartsInDraft()
    {
        var item = BatchedRecognitionSlot.NewBatchedRecognitionSlot("code-87", "name-836");
        Check.Equal(item.Status, BatchedRecognitionSlotStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedRecognitionSlotMovesToPending()
    {
        var item = BatchedRecognitionSlot.NewBatchedRecognitionSlot("code-87", "name-836");
        try
        {
            item.MoveTo(BatchedRecognitionSlotStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedRecognitionSlotStatus.Pending, "status");
    }

    public static void BatchedRecognitionSlotRejectsMovesFromClosed()
    {
        var item = BatchedRecognitionSlot.NewBatchedRecognitionSlot("code-87", "name-836");
        item.Status = BatchedRecognitionSlotStatus.Closed;
        Check.Fails(() => item.MoveTo(BatchedRecognitionSlotStatus.Draft), "move");
    }

    public static void RunAll()
    {
        BatchedRecognitionSlotStartsInDraft();
        BatchedRecognitionSlotMovesToPending();
        BatchedRecognitionSlotRejectsMovesFromClosed();
    }
}
