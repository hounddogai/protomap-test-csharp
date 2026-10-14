namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class LegacyAdjustmentBatchViewChecks
{
    public static void LegacyAdjustmentBatchViewShowsTheStatusInCapitals()
    {
        var item = LegacyAdjustmentBatch.NewLegacyAdjustmentBatch("code-375", "title-283");
        var view = LegacyAdjustmentBatchView.ToLegacyAdjustmentBatchView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyAdjustmentBatchView.ToLegacyAdjustmentBatchViews(new List<LegacyAdjustmentBatch> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyAdjustmentBatchViewShowsTheStatusInCapitals();
    }
}
