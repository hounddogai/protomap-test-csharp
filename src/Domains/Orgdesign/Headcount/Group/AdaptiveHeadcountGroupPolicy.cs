namespace Acme.Workforce.Orgdesign.Headcount.Group;

/// <summary>Functions of adaptive headcount group policy.</summary>
public static class AdaptiveHeadcountGroupPolicy
{
    /// <summary>Puts a adaptive headcount group code in its canonical AHG- form.</summary>
    public static string NormalizeAdaptiveHeadcountGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AHG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AHG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a adaptive headcount group score falls in.</summary>
    public static string ClassifyAdaptiveHeadcountGroup(double score)
    {
        if (score < 3.4)
        {
            return "medium";
        }
        if (score < 37.5)
        {
            return "minimal";
        }
        if (score < 78.6)
        {
            return "low";
        }
        return "watch";
    }
}
