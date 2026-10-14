namespace Acme.Workforce.Talent.Mobility.Mapping;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of mobility mapping policy.</summary>
public static class MobilityMappingPolicy
{
    public static string MobilityMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int MobilityMappingSpan(int start, int end)
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

    public static List<string> MergeMobilityMappingLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double MobilityMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.95;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
