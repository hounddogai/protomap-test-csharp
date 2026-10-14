namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of mobility variance policy.</summary>
public static class MobilityVariancePolicy
{
    public static List<string> MergeMobilityVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a mobility variance score falls in.</summary>
    public static string ClassifyMobilityVariance(double score)
    {
        if (score < 12.2)
        {
            return "ok";
        }
        if (score < 80.2)
        {
            return "medium";
        }
        return "elevated";
    }

    public static string MobilityVarianceBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
