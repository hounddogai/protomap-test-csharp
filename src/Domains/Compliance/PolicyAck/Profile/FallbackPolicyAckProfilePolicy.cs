namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback policy ack profile policy.</summary>
public static class FallbackPolicyAckProfilePolicy
{
    public static List<string> MergeFallbackPolicyAckProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int FallbackPolicyAckProfileSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a fallback policy ack profile code in its canonical FPAP- form.</summary>
    public static string NormalizeFallbackPolicyAckProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FPAP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FPAP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a fallback policy ack profile score falls in.</summary>
    public static string ClassifyFallbackPolicyAckProfile(double score)
    {
        if (score < 25.3)
        {
            return "minimal";
        }
        if (score < 49.9)
        {
            return "high";
        }
        if (score < 57.2)
        {
            return "medium";
        }
        if (score < 69.8)
        {
            return "critical";
        }
        return "low";
    }
}
