namespace Acme.Workforce.Absence.Accrual.Rollup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled accrual allocation policy.</summary>
public static class ScheduledAccrualAllocationPolicy
{
    public static int ScheduledAccrualAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledAccrualAllocation()
    {
        var policy = Retry.DefaultPolicy("scheduled-accrual-allocation");
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

    public static double ScheduledAccrualAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.02;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a scheduled accrual allocation score falls in.</summary>
    public static string ClassifyScheduledAccrualAllocation(double score)
    {
        if (score < 17.1)
        {
            return "critical";
        }
        if (score < 52.9)
        {
            return "high";
        }
        return "minimal";
    }
}
