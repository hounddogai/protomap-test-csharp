namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient succession plan request policy.</summary>
public static class LenientSuccessionPlanRequestPolicy
{
    /// <summary>Puts a lenient succession plan request code in its canonical LSPR- form.</summary>
    public static string NormalizeLenientSuccessionPlanRequestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LSPR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LSPR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string LenientSuccessionPlanRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    public static List<string> MergeLenientSuccessionPlanRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a lenient succession plan request score falls in.</summary>
    public static string ClassifyLenientSuccessionPlanRequest(double score)
    {
        if (score < 56.8)
        {
            return "critical";
        }
        if (score < 67.9)
        {
            return "watch";
        }
        if (score < 95.2)
        {
            return "ok";
        }
        return "minimal";
    }
}
