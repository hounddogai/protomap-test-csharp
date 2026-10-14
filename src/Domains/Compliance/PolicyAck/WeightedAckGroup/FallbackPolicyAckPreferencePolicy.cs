namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of fallback policy ack preference policy.</summary>
public static class FallbackPolicyAckPreferencePolicy
{
    public static double FallbackPolicyAckPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.74;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a fallback policy ack preference code in its canonical FPAP- form.</summary>
    public static string NormalizeFallbackPolicyAckPreferenceCode(string code)
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

    public static List<string> MergeFallbackPolicyAckPreferenceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
