namespace Acme.Workforce.Workplace.Desk.DeskVariance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered desk variance policy.</summary>
public static class TieredDeskVariancePolicy
{
    public static double TieredDeskVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.3;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeTieredDeskVarianceLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a tiered desk variance score falls in.</summary>
    public static string ClassifyTieredDeskVariance(double score)
    {
        if (score < 8.2)
        {
            return "low";
        }
        if (score < 48.1)
        {
            return "elevated";
        }
        if (score < 75.6)
        {
            return "medium";
        }
        return "watch";
    }
}
