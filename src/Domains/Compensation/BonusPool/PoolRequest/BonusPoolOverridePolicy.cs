namespace Acme.Workforce.Compensation.BonusPool.PoolRequest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of bonus pool override policy.</summary>
public static class BonusPoolOverridePolicy
{
    public static int BonusPoolOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static string BonusPoolOverrideBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static double BonusPoolOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.7;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
