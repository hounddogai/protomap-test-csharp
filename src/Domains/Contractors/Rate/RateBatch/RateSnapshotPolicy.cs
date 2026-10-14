namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rate snapshot policy.</summary>
public static class RateSnapshotPolicy
{
    public static List<string> MergeRateSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RateSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.76;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a rate snapshot score falls in.</summary>
    public static string ClassifyRateSnapshot(double score)
    {
        if (score < 11.4)
        {
            return "minimal";
        }
        if (score < 30.6)
        {
            return "medium";
        }
        if (score < 51.5)
        {
            return "elevated";
        }
        return "watch";
    }
}
