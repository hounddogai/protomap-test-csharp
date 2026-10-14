namespace Acme.Workforce.Assets.Tests.Warranty.RecordItem;

using Acme.Workforce.Assets.Warranty.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedWarrantyRecordViewChecks
{
    public static void BatchedWarrantyRecordViewShowsTheStatusInCapitals()
    {
        var item = BatchedWarrantyRecord.NewBatchedWarrantyRecord("code-85", "title-518");
        var view = BatchedWarrantyRecordView.ToBatchedWarrantyRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedWarrantyRecordView.ToBatchedWarrantyRecordViews(new List<BatchedWarrantyRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedWarrantyRecordViewShowsTheStatusInCapitals();
    }
}
