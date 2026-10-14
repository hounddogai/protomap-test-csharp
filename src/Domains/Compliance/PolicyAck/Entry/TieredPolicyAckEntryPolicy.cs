namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered policy ack entry policy.</summary>
public static class TieredPolicyAckEntryPolicy
{
    /// <summary>Puts a tiered policy ack entry code in its canonical TPAE- form.</summary>
    public static string NormalizeTieredPolicyAckEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TPAE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TPAE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeTieredPolicyAckEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
