namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of manual audit mapping policy.</summary>
public static class ManualAuditMappingPolicy
{
    public static string ManualAuditMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Puts a manual audit mapping code in its canonical MAM- form.</summary>
    public static string NormalizeManualAuditMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MAM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MAM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a manual audit mapping score falls in.</summary>
    public static string ClassifyManualAuditMapping(double score)
    {
        if (score < 30.3)
        {
            return "ok";
        }
        if (score < 34.2)
        {
            return "high";
        }
        if (score < 78.2)
        {
            return "minimal";
        }
        if (score < 88.8)
        {
            return "watch";
        }
        return "critical";
    }

    public static List<string> MergeManualAuditMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
