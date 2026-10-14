namespace Acme.Workforce.Orgdesign.Span.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling span plan policy.</summary>
public static class RollingSpanPlanPolicy
{
    /// <summary>Puts a rolling span plan code in its canonical RSP- form.</summary>
    public static string NormalizeRollingSpanPlanCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RSP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RSP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a rolling span plan score falls in.</summary>
    public static string ClassifyRollingSpanPlan(double score)
    {
        if (score < 13.2)
        {
            return "watch";
        }
        if (score < 22.3)
        {
            return "low";
        }
        if (score < 22.6)
        {
            return "high";
        }
        if (score < 94.1)
        {
            return "medium";
        }
        return "ok";
    }

    public static List<string> MergeRollingSpanPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
