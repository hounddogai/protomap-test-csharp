namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class WeightedMeritCycleSegmentChecks
{
    public static void WeightedMeritCycleSegmentStartsInOpen()
    {
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("code-441", "title-829");
        Check.Equal(item.Status, WeightedMeritCycleSegmentStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedMeritCycleSegmentMovesToReopened()
    {
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("code-441", "title-829");
        try
        {
            item.MoveTo(WeightedMeritCycleSegmentStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedMeritCycleSegmentStatus.Reopened, "status");
    }

    public static void WeightedMeritCycleSegmentRejectsMovesFromArchived()
    {
        var item = WeightedMeritCycleSegment.NewWeightedMeritCycleSegment("code-441", "title-829");
        item.Status = WeightedMeritCycleSegmentStatus.Archived;
        Check.Fails(() => item.MoveTo(WeightedMeritCycleSegmentStatus.Open), "move");
    }

    public static void RunAll()
    {
        WeightedMeritCycleSegmentStartsInOpen();
        WeightedMeritCycleSegmentMovesToReopened();
        WeightedMeritCycleSegmentRejectsMovesFromArchived();
    }
}
