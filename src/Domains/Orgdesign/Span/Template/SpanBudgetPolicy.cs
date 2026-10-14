namespace Acme.Workforce.Orgdesign.Span.Template;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span budget policy.</summary>
public static class SpanBudgetPolicy
{
    public static string SpanBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeSpanBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a span budget code in its canonical SB- form.</summary>
    public static string NormalizeSpanBudgetCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a span budget score falls in.</summary>
    public static string ClassifySpanBudget(double score)
    {
        if (score < 10.9)
        {
            return "critical";
        }
        if (score < 11.7)
        {
            return "high";
        }
        if (score < 48.7)
        {
            return "low";
        }
        return "medium";
    }
}
