namespace Acme.Workforce.Compensation.Tests.MeritCycle.Target;

using Acme.Workforce.Compensation.MeritCycle.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class MeritCycleSegmentChecks
{
    public static void MeritCycleSegmentStartsInDraft()
    {
        var item = MeritCycleSegment.NewMeritCycleSegment("code-129", "title-371");
        Check.Equal(item.Status, MeritCycleSegmentStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleSegmentMovesToActive()
    {
        var item = MeritCycleSegment.NewMeritCycleSegment("code-129", "title-371");
        try
        {
            item.MoveTo(MeritCycleSegmentStatus.Active);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleSegmentStatus.Active, "status");
    }

    public static void MeritCycleSegmentRejectsMovesFromClosed()
    {
        var item = MeritCycleSegment.NewMeritCycleSegment("code-129", "title-371");
        item.Status = MeritCycleSegmentStatus.Closed;
        Check.Fails(() => item.MoveTo(MeritCycleSegmentStatus.Draft), "move");
    }

    public static void RunAll()
    {
        MeritCycleSegmentStartsInDraft();
        MeritCycleSegmentMovesToActive();
        MeritCycleSegmentRejectsMovesFromClosed();
    }
}
