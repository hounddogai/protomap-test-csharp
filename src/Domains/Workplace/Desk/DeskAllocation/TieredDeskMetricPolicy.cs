namespace Acme.Workforce.Workplace.Desk.DeskAllocation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered desk metric policy.</summary>
public static class TieredDeskMetricPolicy
{
    public static double TieredDeskMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.24;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredDeskMetric()
    {
        var policy = Retry.DefaultPolicy("tiered-desk-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
