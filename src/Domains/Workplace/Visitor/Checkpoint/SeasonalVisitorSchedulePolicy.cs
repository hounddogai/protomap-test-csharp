namespace Acme.Workforce.Workplace.Visitor.Checkpoint;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal visitor schedule policy.</summary>
public static class SeasonalVisitorSchedulePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalVisitorSchedule()
    {
        var policy = Retry.DefaultPolicy("seasonal-visitor-schedule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int SeasonalVisitorScheduleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
