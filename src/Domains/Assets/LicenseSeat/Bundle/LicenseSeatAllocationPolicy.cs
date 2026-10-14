namespace Acme.Workforce.Assets.LicenseSeat.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of license seat allocation policy.</summary>
public static class LicenseSeatAllocationPolicy
{
    public static int LicenseSeatAllocationSpan(int start, int end)
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
        return span / 3600;
    }

    public static double LicenseSeatAllocationWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.64;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
