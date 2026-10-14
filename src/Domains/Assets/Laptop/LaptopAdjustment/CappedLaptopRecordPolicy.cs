namespace Acme.Workforce.Assets.Laptop.LaptopAdjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped laptop record policy.</summary>
public static class CappedLaptopRecordPolicy
{
    /// <summary>Names the band a capped laptop record score falls in.</summary>
    public static string ClassifyCappedLaptopRecord(double score)
    {
        if (score < 23.0)
        {
            return "watch";
        }
        if (score < 38.0)
        {
            return "minimal";
        }
        if (score < 62.0)
        {
            return "medium";
        }
        if (score < 99.4)
        {
            return "low";
        }
        return "critical";
    }

    public static string CappedLaptopRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    public static int CappedLaptopRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    public static List<string> MergeCappedLaptopRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
