namespace Acme.Workforce.Orgdesign.Tests.Position.Segment;

using Acme.Workforce.Orgdesign.Position.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class PositionSegmentChecks
{
    public static void PositionSegmentStartsInPlanned()
    {
        var item = PositionSegment.NewPositionSegment("code-563", "label-709");
        Check.Equal(item.Status, PositionSegmentStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PositionSegmentMovesToScheduled()
    {
        var item = PositionSegment.NewPositionSegment("code-563", "label-709");
        try
        {
            item.MoveTo(PositionSegmentStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PositionSegmentStatus.Scheduled, "status");
    }

    public static void PositionSegmentRejectsMovesFromCancelled()
    {
        var item = PositionSegment.NewPositionSegment("code-563", "label-709");
        item.Status = PositionSegmentStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PositionSegmentStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PositionSegmentStartsInPlanned();
        PositionSegmentMovesToScheduled();
        PositionSegmentRejectsMovesFromCancelled();
    }
}
