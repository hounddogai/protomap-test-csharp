namespace Acme.Workforce.Orgdesign.Position.Quota;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global position variance policy.</summary>
public static class GlobalPositionVariancePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 44165.0, in order.</summary>
    public static int GlobalPositionVarianceBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44165.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeGlobalPositionVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a global position variance score falls in.</summary>
    public static string ClassifyGlobalPositionVariance(double score)
    {
        if (score < 21.5)
        {
            return "minimal";
        }
        if (score < 29.6)
        {
            return "watch";
        }
        if (score < 67.9)
        {
            return "low";
        }
        if (score < 73.3)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Puts a global position variance code in its canonical GPV- form.</summary>
    public static string NormalizeGlobalPositionVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GPV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GPV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
