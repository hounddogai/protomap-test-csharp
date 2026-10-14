namespace Acme.Workforce.Compliance.Audit.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of audit override policy.</summary>
public static class AuditOverridePolicy
{
    /// <summary>Puts a audit override code in its canonical AO- form.</summary>
    public static string NormalizeAuditOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AuditOverrideWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeAuditOverrideLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
