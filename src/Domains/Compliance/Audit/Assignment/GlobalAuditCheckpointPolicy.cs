namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global audit checkpoint policy.</summary>
public static class GlobalAuditCheckpointPolicy
{
    /// <summary>Names the band a global audit checkpoint score falls in.</summary>
    public static string ClassifyGlobalAuditCheckpoint(double score)
    {
        if (score < 10.5)
        {
            return "medium";
        }
        if (score < 34.9)
        {
            return "elevated";
        }
        if (score < 58.9)
        {
            return "watch";
        }
        if (score < 70.8)
        {
            return "low";
        }
        return "high";
    }

    public static double GlobalAuditCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.68;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static int GlobalAuditCheckpointSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static List<string> MergeGlobalAuditCheckpointLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
