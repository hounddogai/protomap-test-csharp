namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched mobility manifest policy.</summary>
public static class BatchedMobilityManifestPolicy
{
    public static List<string> MergeBatchedMobilityManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int BatchedMobilityManifestSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a batched mobility manifest code in its canonical BMM- form.</summary>
    public static string NormalizeBatchedMobilityManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BMM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BMM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a batched mobility manifest score falls in.</summary>
    public static string ClassifyBatchedMobilityManifest(double score)
    {
        if (score < 54.5)
        {
            return "ok";
        }
        if (score < 81.1)
        {
            return "low";
        }
        if (score < 89.0)
        {
            return "high";
        }
        return "minimal";
    }
}
