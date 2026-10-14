namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive retro pay metric policy.</summary>
public static class AdaptiveRetroPayMetricPolicy
{
    /// <summary>Names the band a adaptive retro pay metric score falls in.</summary>
    public static string ClassifyAdaptiveRetroPayMetric(double score)
    {
        if (score < 10.9)
        {
            return "critical";
        }
        if (score < 35.5)
        {
            return "ok";
        }
        if (score < 74.3)
        {
            return "minimal";
        }
        if (score < 88.0)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 36369.0, in order.</summary>
    public static int AdaptiveRetroPayMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36369.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a adaptive retro pay metric code in its canonical ARPM- form.</summary>
    public static string NormalizeAdaptiveRetroPayMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAdaptiveRetroPayMetricLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
