namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdjustmentBatchViewChecks
{
    public static void AdjustmentBatchViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentBatch.NewAdjustmentBatch("code-467", "label-64");
        var view = AdjustmentBatchView.ToAdjustmentBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentBatchView.ToAdjustmentBatchViews(new List<AdjustmentBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentBatchViewShowsTheStatusInCapitals();
    }
}
