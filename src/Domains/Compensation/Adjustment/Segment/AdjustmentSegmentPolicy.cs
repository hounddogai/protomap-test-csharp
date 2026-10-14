namespace Acme.Workforce.Compensation.Adjustment.Segment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment segment policy.</summary>
public static class AdjustmentSegmentPolicy
{
    public static double AdjustmentSegmentWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int AdjustmentSegmentSpan(int start, int end)
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

    public static string AdjustmentSegmentBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
