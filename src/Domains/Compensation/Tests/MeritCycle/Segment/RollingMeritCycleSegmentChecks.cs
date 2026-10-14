namespace Acme.Workforce.Compensation.Tests.MeritCycle.Segment;

using Acme.Workforce.Compensation.MeritCycle.Segment;

/// <summary>Checks of segment code, run by the project's runner.</summary>
public static class RollingMeritCycleSegmentChecks
{
    public static void RollingMeritCycleSegmentStartsInDraft()
    {
        var item = RollingMeritCycleSegment.NewRollingMeritCycleSegment("code-392", "label-236");
        Check.Equal(item.Status, RollingMeritCycleSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void RollingMeritCycleSegmentMovesToActive()
    {
        var item = RollingMeritCycleSegment.NewRollingMeritCycleSegment("code-392", "label-236");
        try
        {
            item.MoveTo(RollingMeritCycleSegmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, RollingMeritCycleSegmentStatus.Active, "status");
    }

    public static void RollingMeritCycleSegmentRejectsMovesFromClosed()
    {
        var item = RollingMeritCycleSegment.NewRollingMeritCycleSegment("code-392", "label-236");
        item.Status = RollingMeritCycleSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(RollingMeritCycleSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        RollingMeritCycleSegmentStartsInDraft();
        RollingMeritCycleSegmentMovesToActive();
        RollingMeritCycleSegmentRejectsMovesFromClosed();
    }
}
