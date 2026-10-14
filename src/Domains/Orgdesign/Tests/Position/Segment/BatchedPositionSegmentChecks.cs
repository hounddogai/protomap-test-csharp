namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class BatchedPositionSegmentChecks
{
    public static void BatchedPositionSegmentStartsInNew()
    {
        var item = BatchedPositionSegment.NewBatchedPositionSegment("code-246", "title-543");
        Check.Equal(item.Status, BatchedPositionSegmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedPositionSegmentMovesToQueued()
    {
        var item = BatchedPositionSegment.NewBatchedPositionSegment("code-246", "title-543");
        try
        {
            item.MoveTo(BatchedPositionSegmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedPositionSegmentStatus.Queued, "status");
    }

    public static void BatchedPositionSegmentRejectsMovesFromFailed()
    {
        var item = BatchedPositionSegment.NewBatchedPositionSegment("code-246", "title-543");
        item.Status = BatchedPositionSegmentStatus.Failed;
        Check.Fails(() => item.MoveTo(BatchedPositionSegmentStatus.New), "move");
    }

    public static void RunAll()
    {
        BatchedPositionSegmentStartsInNew();
        BatchedPositionSegmentMovesToQueued();
        BatchedPositionSegmentRejectsMovesFromFailed();
    }
}
