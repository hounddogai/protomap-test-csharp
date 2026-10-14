namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped laptop rollup policy.</summary>
public static class CappedLaptopRollupPolicy
{
    public static int CappedLaptopRollupSpan(int start, int end)
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
        return span / 60;
    }

    public static double CappedLaptopRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.52;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a capped laptop rollup score falls in.</summary>
    public static string ClassifyCappedLaptopRollup(double score)
    {
        if (score < 65.0)
        {
            return "ok";
        }
        if (score < 85.5)
        {
            return "low";
        }
        return "critical";
    }

    public static List<string> MergeCappedLaptopRollupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
