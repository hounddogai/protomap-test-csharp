namespace Acme.Workforce.Talent.CareerPath.Slot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of career path digest policy.</summary>
public static class CareerPathDigestPolicy
{
    public static double CareerPathDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.31;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeCareerPathDigestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a career path digest score falls in.</summary>
    public static string ClassifyCareerPathDigest(double score)
    {
        if (score < 70.5)
        {
            return "low";
        }
        if (score < 89.0)
        {
            return "minimal";
        }
        if (score < 90.2)
        {
            return "medium";
        }
        if (score < 98.6)
        {
            return "ok";
        }
        return "high";
    }
}
