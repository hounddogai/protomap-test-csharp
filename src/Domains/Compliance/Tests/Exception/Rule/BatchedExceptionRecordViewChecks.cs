namespace Acme.Workforce.Compliance.Tests.Exception.Rule;

using Acme.Workforce.Compliance.Exception.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class BatchedExceptionRecordViewChecks
{
    public static void BatchedExceptionRecordViewShowsTheStatusInCapitals()
    {
        var item = BatchedExceptionRecord.NewBatchedExceptionRecord("code-361", "name-538");
        var view = BatchedExceptionRecordView.ToBatchedExceptionRecordView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedExceptionRecordView.ToBatchedExceptionRecordViews(new List<BatchedExceptionRecord> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedExceptionRecordViewShowsTheStatusInCapitals();
    }
}
