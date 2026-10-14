namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive occupancy template policy.</summary>
public static class AdaptiveOccupancyTemplatePolicy
{
    /// <summary>Names the band a adaptive occupancy template score falls in.</summary>
    public static string ClassifyAdaptiveOccupancyTemplate(double score)
    {
        if (score < 30.1)
        {
            return "high";
        }
        if (score < 45.8)
        {
            return "watch";
        }
        if (score < 77.6)
        {
            return "low";
        }
        return "medium";
    }

    public static string AdaptiveOccupancyTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Puts a adaptive occupancy template code in its canonical AOT- form.</summary>
    public static string NormalizeAdaptiveOccupancyTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AOT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AOT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
