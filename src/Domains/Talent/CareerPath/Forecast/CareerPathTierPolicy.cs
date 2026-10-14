namespace Acme.Workforce.Talent.CareerPath.Forecast;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path tier policy.</summary>
public static class CareerPathTierPolicy
{
    /// <summary>Names the band a career path tier score falls in.</summary>
    public static string ClassifyCareerPathTier(double score)
    {
        if (score < 4.7)
        {
            return "medium";
        }
        if (score < 26.3)
        {
            return "low";
        }
        return "watch";
    }

    public static int CareerPathTierSpan(int start, int end)
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
        return span / 86400;
    }

    public static double CareerPathTierWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.84;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeCareerPathTierLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
