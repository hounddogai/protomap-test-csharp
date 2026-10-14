namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool segment policy.</summary>
public static class PoolSegmentPolicy
{
    public static int PoolSegmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    public static string PoolSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
