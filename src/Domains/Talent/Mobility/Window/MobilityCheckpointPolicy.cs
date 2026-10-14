namespace Acme.Workforce.Talent.Mobility.Window;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of mobility checkpoint policy.</summary>
public static class MobilityCheckpointPolicy
{
    /// <summary>Puts a mobility checkpoint code in its canonical MC- form.</summary>
    public static string NormalizeMobilityCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a mobility checkpoint score falls in.</summary>
    public static string ClassifyMobilityCheckpoint(double score)
    {
        if (score < 4.1)
        {
            return "minimal";
        }
        if (score < 69.7)
        {
            return "elevated";
        }
        if (score < 77.6)
        {
            return "low";
        }
        return "ok";
    }

    public static List<string> MergeMobilityCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int MobilityCheckpointSpan(int start, int end)
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
        return span / 3600;
    }
}
