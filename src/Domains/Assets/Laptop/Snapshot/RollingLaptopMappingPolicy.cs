namespace Acme.Workforce.Assets.Laptop.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of rolling laptop mapping policy.</summary>
public static class RollingLaptopMappingPolicy
{
    /// <summary>Names the band a rolling laptop mapping score falls in.</summary>
    public static string ClassifyRollingLaptopMapping(double score)
    {
        if (score < 18.6)
        {
            return "medium";
        }
        if (score < 34.2)
        {
            return "elevated";
        }
        if (score < 96.6)
        {
            return "minimal";
        }
        return "critical";
    }

    public static string RollingLaptopMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static List<string> MergeRollingLaptopMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double RollingLaptopMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.35;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
