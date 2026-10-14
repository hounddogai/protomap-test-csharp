namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BatchedAdjustmentRequestViewChecks
{
    public static void BatchedAdjustmentRequestViewShowsTheStatusInCapitals()
    {
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("code-109", "title-32");
        var view = BatchedAdjustmentRequestView.ToBatchedAdjustmentRequestView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedAdjustmentRequestView.ToBatchedAdjustmentRequestViews(new List<BatchedAdjustmentRequest> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedAdjustmentRequestViewShowsTheStatusInCapitals();
    }
}
