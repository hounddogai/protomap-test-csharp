namespace Acme.Workforce.Workplace.AccessBadge.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of access badge target policy.</summary>
public static class AccessBadgeTargetPolicy
{
    public static string AccessBadgeTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int AccessBadgeTargetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static double AccessBadgeTargetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.13;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
