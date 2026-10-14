namespace Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Functions of primary floor plan notice policy.</summary>
public static class PrimaryFloorPlanNoticePolicy
{
    /// <summary>Puts a primary floor plan notice code in its canonical PFPN- form.</summary>
    public static string NormalizePrimaryFloorPlanNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PFPN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PFPN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a primary floor plan notice score falls in.</summary>
    public static string ClassifyPrimaryFloorPlanNotice(double score)
    {
        if (score < 68.4)
        {
            return "elevated";
        }
        if (score < 88.5)
        {
            return "watch";
        }
        return "ok";
    }
}
