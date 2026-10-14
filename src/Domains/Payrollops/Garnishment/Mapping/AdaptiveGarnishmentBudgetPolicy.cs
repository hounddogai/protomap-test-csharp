namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive garnishment budget policy.</summary>
public static class AdaptiveGarnishmentBudgetPolicy
{
    /// <summary>Names the band a adaptive garnishment budget score falls in.</summary>
    public static string ClassifyAdaptiveGarnishmentBudget(double score)
    {
        if (score < 15.1)
        {
            return "watch";
        }
        if (score < 26.4)
        {
            return "high";
        }
        if (score < 61.8)
        {
            return "ok";
        }
        if (score < 67.1)
        {
            return "elevated";
        }
        return "low";
    }

    public static List<string> MergeAdaptiveGarnishmentBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double AdaptiveGarnishmentBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.6;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a adaptive garnishment budget code in its canonical AGB- form.</summary>
    public static string NormalizeAdaptiveGarnishmentBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AGB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AGB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
