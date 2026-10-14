namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of visitor summary policy.</summary>
public static class VisitorSummaryPolicy
{
    /// <summary>Names the band a visitor summary score falls in.</summary>
    public static string ClassifyVisitorSummary(double score)
    {
        if (score < 78.8)
        {
            return "medium";
        }
        if (score < 84.4)
        {
            return "critical";
        }
        return "watch";
    }

    public static double VisitorSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.01;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a visitor summary code in its canonical VS- form.</summary>
    public static string NormalizeVisitorSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("VS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"VS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeVisitorSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
