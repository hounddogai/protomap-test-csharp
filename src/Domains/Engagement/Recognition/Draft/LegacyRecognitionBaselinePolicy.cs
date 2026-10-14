namespace Acme.Workforce.Engagement.Recognition.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy recognition baseline policy.</summary>
public static class LegacyRecognitionBaselinePolicy
{
    public static double LegacyRecognitionBaselineWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 3.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a legacy recognition baseline score falls in.</summary>
    public static string ClassifyLegacyRecognitionBaseline(double score)
    {
        if (score < 8.8)
        {
            return "critical";
        }
        if (score < 13.8)
        {
            return "minimal";
        }
        if (score < 17.1)
        {
            return "high";
        }
        return "watch";
    }

    /// <summary>Puts a legacy recognition baseline code in its canonical LRB- form.</summary>
    public static string NormalizeLegacyRecognitionBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LRB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LRB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
