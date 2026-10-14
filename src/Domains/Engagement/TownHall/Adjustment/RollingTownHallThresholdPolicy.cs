namespace Acme.Workforce.Engagement.TownHall.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of rolling town hall threshold policy.</summary>
public static class RollingTownHallThresholdPolicy
{
    public static int RollingTownHallThresholdSpan(int start, int end)
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
        return span / 60;
    }

    public static double RollingTownHallThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.81;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
