namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of pay band score policy.</summary>
public static class PayBandScorePolicy
{
    public static double PayBandScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.4;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int PayBandScoreSpan(int start, int end)
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
        return span / 86400;
    }
}
