namespace Acme.Workforce.Compensation.EquityGrant.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant tier policy.</summary>
public static class EquityGrantTierPolicy
{
    public static double EquityGrantTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.56;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int EquityGrantTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
