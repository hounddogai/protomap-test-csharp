namespace Acme.Workforce.Talent.Pool.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tiered pool group policy.</summary>
public static class TieredPoolGroupPolicy
{
    public static string TieredPoolGroupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static double TieredPoolGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.19;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int TieredPoolGroupSpan(int start, int end)
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
