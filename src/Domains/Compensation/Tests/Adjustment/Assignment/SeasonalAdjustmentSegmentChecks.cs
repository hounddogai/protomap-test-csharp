namespace Acme.Workforce.Compensation.Tests.Adjustment.Assignment;

using Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class SeasonalAdjustmentSegmentChecks
{
    public static void SeasonalAdjustmentSegmentStartsInPlanned()
    {
        var item = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment("code-873", "label-713");
        Check.Equal(item.Status, SeasonalAdjustmentSegmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAdjustmentSegmentMovesToScheduled()
    {
        var item = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment("code-873", "label-713");
        try
        {
            item.MoveTo(SeasonalAdjustmentSegmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAdjustmentSegmentStatus.Scheduled, "status");
    }

    public static void SeasonalAdjustmentSegmentRejectsMovesFromCancelled()
    {
        var item = SeasonalAdjustmentSegment.NewSeasonalAdjustmentSegment("code-873", "label-713");
        item.Status = SeasonalAdjustmentSegmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(SeasonalAdjustmentSegmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        SeasonalAdjustmentSegmentStartsInPlanned();
        SeasonalAdjustmentSegmentMovesToScheduled();
        SeasonalAdjustmentSegmentRejectsMovesFromCancelled();
    }
}
