namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band schedule policy.</summary>
public static class PayBandSchedulePolicy
{
    public static double PayBandScheduleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PayBandScheduleSpan(int start, int end)
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
