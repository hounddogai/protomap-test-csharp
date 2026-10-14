namespace Acme.Workforce.Compensation.Adjustment.Assignment;

/// <summary>Functions of seasonal adjustment segment policy.</summary>
public static class SeasonalAdjustmentSegmentPolicy
{
    /// <summary>Puts a seasonal adjustment segment code in its canonical SAS- form.</summary>
    public static string NormalizeSeasonalAdjustmentSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a seasonal adjustment segment score falls in.</summary>
    public static string ClassifySeasonalAdjustmentSegment(double score)
    {
        if (score < 30.7)
        {
            return "medium";
        }
        if (score < 63.2)
        {
            return "ok";
        }
        return "low";
    }
}
