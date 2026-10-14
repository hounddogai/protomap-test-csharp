namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BatchedPositionSegmentViewChecks
{
    public static void BatchedPositionSegmentViewShowsTheStatusInCapitals()
    {
        var item = BatchedPositionSegment.NewBatchedPositionSegment("code-246", "title-543");
        var view = BatchedPositionSegmentView.ToBatchedPositionSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(BatchedPositionSegmentView.ToBatchedPositionSegmentViews(new List<BatchedPositionSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        BatchedPositionSegmentViewShowsTheStatusInCapitals();
    }
}
