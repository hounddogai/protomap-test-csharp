namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor baseline policy.</summary>
public static class VisitorBaselinePolicy
{
    public static List<string> MergeVisitorBaselineLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Counts how many amounts fit in a budget of 56183.0, in order.</summary>
    public static int VisitorBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 56183.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a visitor baseline score falls in.</summary>
    public static string ClassifyVisitorBaseline(double score)
    {
        if (score < 68.7)
        {
            return "critical";
        }
        if (score < 78.1)
        {
            return "minimal";
        }
        if (score < 95.7)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Puts a visitor baseline code in its canonical VB- form.</summary>
    public static string NormalizeVisitorBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
