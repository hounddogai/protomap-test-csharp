namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of weighted span mapping policy.</summary>
public static class WeightedSpanMappingPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 95859.0, in order.</summary>
    public static int WeightedSpanMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95859.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeWeightedSpanMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a weighted span mapping code in its canonical WSM- form.</summary>
    public static string NormalizeWeightedSpanMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WSM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WSM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a weighted span mapping score falls in.</summary>
    public static string ClassifyWeightedSpanMapping(double score)
    {
        if (score < 35.8)
        {
            return "medium";
        }
        if (score < 45.4)
        {
            return "high";
        }
        if (score < 50.0)
        {
            return "ok";
        }
        if (score < 94.8)
        {
            return "elevated";
        }
        return "minimal";
    }
}
