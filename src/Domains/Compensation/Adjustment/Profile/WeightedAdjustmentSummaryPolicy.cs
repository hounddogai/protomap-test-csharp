namespace Acme.Workforce.Compensation.Adjustment.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted adjustment summary policy.</summary>
public static class WeightedAdjustmentSummaryPolicy
{
    public static int WeightedAdjustmentSummarySpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a weighted adjustment summary score falls in.</summary>
    public static string ClassifyWeightedAdjustmentSummary(double score)
    {
        if (score < 5.6)
        {
            return "watch";
        }
        if (score < 20.1)
        {
            return "medium";
        }
        if (score < 44.0)
        {
            return "minimal";
        }
        if (score < 82.4)
        {
            return "ok";
        }
        return "high";
    }

    public static string WeightedAdjustmentSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
