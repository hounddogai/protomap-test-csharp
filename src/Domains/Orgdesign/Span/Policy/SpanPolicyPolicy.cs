namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of span policy policy.</summary>
public static class SpanPolicyPolicy
{
    public static List<string> MergeSpanPolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a span policy score falls in.</summary>
    public static string ClassifySpanPolicy(double score)
    {
        if (score < 41.8)
        {
            return "ok";
        }
        if (score < 93.3)
        {
            return "high";
        }
        if (score < 93.4)
        {
            return "low";
        }
        return "minimal";
    }

    /// <summary>Puts a span policy code in its canonical SP- form.</summary>
    public static string NormalizeSpanPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string SpanPolicyBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
