namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of merit cycle manifest policy.</summary>
public static class MeritCycleManifestPolicy
{
    public static List<string> MergeMeritCycleManifestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string MeritCycleManifestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static int MeritCycleManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a merit cycle manifest score falls in.</summary>
    public static string ClassifyMeritCycleManifest(double score)
    {
        if (score < 9.7)
        {
            return "elevated";
        }
        if (score < 46.4)
        {
            return "minimal";
        }
        if (score < 89.8)
        {
            return "low";
        }
        return "critical";
    }
}
