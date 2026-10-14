namespace Acme.Workforce.Assets.Tests.Warranty.RecordItem;

using Acme.Workforce.Assets.Warranty.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedWarrantyRecordChecks
{
    public static void BatchedWarrantyRecordStartsInOpen()
    {
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("code-85", "title-518");
        Check.Equal(item.Status, BatchedWarrantyRecordStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedWarrantyRecordMovesToInReview()
    {
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("code-85", "title-518");
        try
        {
            item.MoveTo(BatchedWarrantyRecordStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedWarrantyRecordStatus.InReview, "status");
    }

    public static void BatchedWarrantyRecordRejectsMovesFromArchived()
    {
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("code-85", "title-518");
        item.Status = BatchedWarrantyRecordStatus.Archived;
        Check.Fails(() => item.MoveTo(BatchedWarrantyRecordStatus.Open), "move");
    }

    public static void RunAll()
    {
        BatchedWarrantyRecordStartsInOpen();
        BatchedWarrantyRecordMovesToInReview();
        BatchedWarrantyRecordRejectsMovesFromArchived();
    }
}
