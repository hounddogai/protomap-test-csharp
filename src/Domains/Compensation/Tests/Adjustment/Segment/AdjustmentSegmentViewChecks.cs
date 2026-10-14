namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentSegmentViewChecks
{
    public static void AdjustmentSegmentViewShowsTheStatusInCapitals()
    {
        var item = AdjustmentSegment.NewAdjustmentSegment("code-150", "label-84");
        var view = AdjustmentSegmentView.ToAdjustmentSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(AdjustmentSegmentView.ToAdjustmentSegmentViews(new List<AdjustmentSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        AdjustmentSegmentViewShowsTheStatusInCapitals();
    }
}
