namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of bonus pool request policy.</summary>
public static class BonusPoolRequestPolicy
{
    public static double BonusPoolRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string BonusPoolRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static int BonusPoolRequestSpan(int start, int end)
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
