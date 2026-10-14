namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of audit quota policy.</summary>
public static class AuditQuotaPolicy
{
    /// <summary>Puts a audit quota code in its canonical AQ- form.</summary>
    public static string NormalizeAuditQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAuditQuotaLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
