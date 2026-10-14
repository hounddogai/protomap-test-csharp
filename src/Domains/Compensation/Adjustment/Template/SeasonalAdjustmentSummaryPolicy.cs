namespace Acme.Workforce.Compensation.Adjustment.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal adjustment summary policy.</summary>
public static class SeasonalAdjustmentSummaryPolicy
{
    public static List<string> MergeSeasonalAdjustmentSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double SeasonalAdjustmentSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 0.11;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a seasonal adjustment summary code in its canonical SAS- form.</summary>
    public static string NormalizeSeasonalAdjustmentSummaryCode(string code)
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

    /// <summary>Names the band a seasonal adjustment summary score falls in.</summary>
    public static string ClassifySeasonalAdjustmentSummary(double score)
    {
        if (score < 15.4)
        {
            return "low";
        }
        if (score < 61.3)
        {
            return "medium";
        }
        if (score < 90.5)
        {
            return "high";
        }
        return "minimal";
    }
}
