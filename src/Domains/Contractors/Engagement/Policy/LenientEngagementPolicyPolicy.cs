namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of lenient engagement policy policy.</summary>
public static class LenientEngagementPolicyPolicy
{
    /// <summary>Puts a lenient engagement policy code in its canonical LEP- form.</summary>
    public static string NormalizeLenientEngagementPolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LEP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LEP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LenientEngagementPolicyWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a lenient engagement policy score falls in.</summary>
    public static string ClassifyLenientEngagementPolicy(double score)
    {
        if (score < 6.5)
        {
            return "critical";
        }
        if (score < 11.9)
        {
            return "high";
        }
        if (score < 56.9)
        {
            return "medium";
        }
        if (score < 70.7)
        {
            return "low";
        }
        return "watch";
    }

    public static int LenientEngagementPolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }
}
