namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of accrual digest policy.</summary>
public static class AccrualDigestPolicy
{
    public static double AccrualDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string AccrualDigestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int AccrualDigestSpan(int start, int end)
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

    /// <summary>Names the band a accrual digest score falls in.</summary>
    public static string ClassifyAccrualDigest(double score)
    {
        if (score < 38.0)
        {
            return "minimal";
        }
        if (score < 42.0)
        {
            return "watch";
        }
        if (score < 59.5)
        {
            return "medium";
        }
        return "low";
    }
}
