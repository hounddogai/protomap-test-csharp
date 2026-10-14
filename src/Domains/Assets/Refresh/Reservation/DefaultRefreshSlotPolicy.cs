namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of default refresh slot policy.</summary>
public static class DefaultRefreshSlotPolicy
{
    public static int DefaultRefreshSlotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    public static double DefaultRefreshSlotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string DefaultRefreshSlotBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
