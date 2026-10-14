namespace Acme.Workforce.Contractors.Engagement.Checkpoint;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending engagement checkpoint policy.</summary>
public static class PendingEngagementCheckpointPolicy
{
    /// <summary>Puts a pending engagement checkpoint code in its canonical PEC- form.</summary>
    public static string NormalizePendingEngagementCheckpointCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PEC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PEC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergePendingEngagementCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
