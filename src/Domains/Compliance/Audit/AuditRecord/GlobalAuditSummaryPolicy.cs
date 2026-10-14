namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global audit summary policy.</summary>
public static class GlobalAuditSummaryPolicy
{
    public static int GlobalAuditSummarySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a global audit summary code in its canonical GAS- form.</summary>
    public static string NormalizeGlobalAuditSummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeGlobalAuditSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
