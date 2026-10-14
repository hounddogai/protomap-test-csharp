namespace Acme.Workforce.Compensation.Adjustment.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive adjustment target policy.</summary>
public static class AdaptiveAdjustmentTargetPolicy
{
    public static int AdaptiveAdjustmentTargetSpan(int start, int end)
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

    public static string AdaptiveAdjustmentTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
