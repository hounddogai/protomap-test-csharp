namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adjustment mapping policy.</summary>
public static class AdjustmentMappingPolicy
{
    /// <summary>Names the band a adjustment mapping score falls in.</summary>
    public static string ClassifyAdjustmentMapping(double score)
    {
        if (score < 49.8)
        {
            return "watch";
        }
        if (score < 69.2)
        {
            return "low";
        }
        if (score < 76.2)
        {
            return "medium";
        }
        return "high";
    }

    public static string AdjustmentMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
