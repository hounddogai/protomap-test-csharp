namespace Acme.Workforce.Compliance.Audit.AuditMapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of default audit mapping policy.</summary>
public static class DefaultAuditMappingPolicy
{
    /// <summary>Puts a default audit mapping code in its canonical DAM- form.</summary>
    public static string NormalizeDefaultAuditMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DAM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DAM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a default audit mapping score falls in.</summary>
    public static string ClassifyDefaultAuditMapping(double score)
    {
        if (score < 19.3)
        {
            return "high";
        }
        if (score < 37.8)
        {
            return "watch";
        }
        if (score < 43.0)
        {
            return "ok";
        }
        if (score < 61.1)
        {
            return "medium";
        }
        return "elevated";
    }

    public static int DefaultAuditMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 86400;
    }

    public static List<string> MergeDefaultAuditMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
