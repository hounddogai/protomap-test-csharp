namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of batched desk tier policy.</summary>
public static class BatchedDeskTierPolicy
{
    /// <summary>Names the band a batched desk tier score falls in.</summary>
    public static string ClassifyBatchedDeskTier(double score)
    {
        if (score < 9.8)
        {
            return "medium";
        }
        if (score < 28.0)
        {
            return "high";
        }
        if (score < 80.0)
        {
            return "critical";
        }
        if (score < 94.9)
        {
            return "minimal";
        }
        return "low";
    }

    public static int BatchedDeskTierSpan(int start, int end)
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

    public static double BatchedDeskTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 3.0;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeBatchedDeskTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
