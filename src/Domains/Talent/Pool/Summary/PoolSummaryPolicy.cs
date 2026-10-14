namespace Acme.Workforce.Talent.Pool.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool summary policy.</summary>
public static class PoolSummaryPolicy
{
    public static string PoolSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int PoolSummarySpan(int start, int end)
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
