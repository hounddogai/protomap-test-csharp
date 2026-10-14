namespace Acme.Workforce.Talent.Pool.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool preference policy.</summary>
public static class PoolPreferencePolicy
{
    public static double PoolPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PoolPreferenceSpan(int start, int end)
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
        return span / 86400;
    }
}
