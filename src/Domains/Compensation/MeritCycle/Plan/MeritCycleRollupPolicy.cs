namespace Acme.Workforce.Compensation.MeritCycle.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of merit cycle rollup policy.</summary>
public static class MeritCycleRollupPolicy
{
    public static double MeritCycleRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.18;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int MeritCycleRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    public static string MeritCycleRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
