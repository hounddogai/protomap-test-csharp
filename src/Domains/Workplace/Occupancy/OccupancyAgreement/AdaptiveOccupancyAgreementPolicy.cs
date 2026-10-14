namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive occupancy agreement policy.</summary>
public static class AdaptiveOccupancyAgreementPolicy
{
    public static List<string> MergeAdaptiveOccupancyAgreementLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a adaptive occupancy agreement score falls in.</summary>
    public static string ClassifyAdaptiveOccupancyAgreement(double score)
    {
        if (score < 33.0)
        {
            return "high";
        }
        if (score < 51.8)
        {
            return "critical";
        }
        if (score < 72.6)
        {
            return "low";
        }
        if (score < 85.4)
        {
            return "minimal";
        }
        return "elevated";
    }
}
