namespace Acme.Workforce.Talent.Pool.Plan;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pool policy policy.</summary>
public static class PoolPolicyPolicy
{
    public static int PoolPolicySpan(int start, int end)
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
        return span / 60;
    }

    public static string PoolPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
