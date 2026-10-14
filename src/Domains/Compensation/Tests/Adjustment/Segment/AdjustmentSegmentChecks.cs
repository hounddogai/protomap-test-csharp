namespace Acme.Workforce.Compensation.Tests.Adjustment.Segment;

using Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class AdjustmentSegmentChecks
{
    public static void AdjustmentSegmentStartsInNew()
    {
        var item = AdjustmentSegment.NewAdjustmentSegment("code-150", "label-84");
        Check.Equal(item.Status, AdjustmentSegmentStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AdjustmentSegmentMovesToQueued()
    {
        var item = AdjustmentSegment.NewAdjustmentSegment("code-150", "label-84");
        try
        {
            item.MoveTo(AdjustmentSegmentStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AdjustmentSegmentStatus.Queued, "status");
    }

    public static void AdjustmentSegmentRejectsMovesFromFailed()
    {
        var item = AdjustmentSegment.NewAdjustmentSegment("code-150", "label-84");
        item.Status = AdjustmentSegmentStatus.Failed;
        Check.Fails(() => item.MoveTo(AdjustmentSegmentStatus.New), "move");
    }

    public static void RunAll()
    {
        AdjustmentSegmentStartsInNew();
        AdjustmentSegmentMovesToQueued();
        AdjustmentSegmentRejectsMovesFromFailed();
    }
}
