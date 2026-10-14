namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of fallback pool metric policy.</summary>
public static class FallbackPoolMetricPolicy
{
    public static double FallbackPoolMetricWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int FallbackPoolMetricSpan(int start, int end)
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
        return span / 86400;
    }
}
