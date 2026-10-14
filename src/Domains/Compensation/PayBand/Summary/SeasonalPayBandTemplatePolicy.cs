namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal pay band template policy.</summary>
public static class SeasonalPayBandTemplatePolicy
{
    /// <summary>Names the band a seasonal pay band template score falls in.</summary>
    public static string ClassifySeasonalPayBandTemplate(double score)
    {
        if (score < 73.6)
        {
            return "watch";
        }
        if (score < 77.1)
        {
            return "low";
        }
        if (score < 82.1)
        {
            return "ok";
        }
        if (score < 96.2)
        {
            return "high";
        }
        return "medium";
    }

    public static double SeasonalPayBandTemplateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.8;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeSeasonalPayBandTemplateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a seasonal pay band template code in its canonical SPBT- form.</summary>
    public static string NormalizeSeasonalPayBandTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPBT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPBT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
