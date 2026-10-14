namespace Acme.Workforce.Compensation.Tests.Adjustment.Summary;

using Acme.Workforce.Compensation.Adjustment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class BatchedAdjustmentClaimViewChecks
{
    public static void BatchedAdjustmentClaimViewShowsTheStatusInCapitals()
    {
        var item = BatchedAdjustmentClaim.NewBatchedAdjustmentClaim("code-929", "title-657");
        var view = BatchedAdjustmentClaimView.ToBatchedAdjustmentClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedAdjustmentClaimView.ToBatchedAdjustmentClaimViews(new List<BatchedAdjustmentClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedAdjustmentClaimViewShowsTheStatusInCapitals();
    }
}
