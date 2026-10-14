namespace Acme.Workforce.Orgdesign.Tests.Headcount.Schedule;

using Acme.Workforce.Orgdesign.Headcount.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class WeightedHeadcountScheduleChecks
{
    public static void WeightedHeadcountScheduleStartsInDraft()
    {
        var item = WeightedHeadcountSchedule.NewWeightedHeadcountSchedule("code-526", "name-831");
        Check.Equal(item.Status, WeightedHeadcountScheduleStatus.Draft, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void WeightedHeadcountScheduleMovesToPending()
    {
        var item = WeightedHeadcountSchedule.NewWeightedHeadcountSchedule("code-526", "name-831");
        try
        {
            item.MoveTo(WeightedHeadcountScheduleStatus.Pending);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, WeightedHeadcountScheduleStatus.Pending, "status");
    }

    public static void WeightedHeadcountScheduleRejectsMovesFromClosed()
    {
        var item = WeightedHeadcountSchedule.NewWeightedHeadcountSchedule("code-526", "name-831");
        item.Status = WeightedHeadcountScheduleStatus.Closed;
        Check.Fails(() => item.MoveTo(WeightedHeadcountScheduleStatus.Draft), "move");
    }

    public static void RunAll()
    {
        WeightedHeadcountScheduleStartsInDraft();
        WeightedHeadcountScheduleMovesToPending();
        WeightedHeadcountScheduleRejectsMovesFromClosed();
    }
}
