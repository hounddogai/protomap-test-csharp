namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of laptop record policy.</summary>
public static class LaptopRecordPolicy
{
    public static int LaptopRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    public static List<string> MergeLaptopRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a laptop record score falls in.</summary>
    public static string ClassifyLaptopRecord(double score)
    {
        if (score < 7.5)
        {
            return "minimal";
        }
        if (score < 49.4)
        {
            return "high";
        }
        if (score < 54.6)
        {
            return "medium";
        }
        if (score < 80.6)
        {
            return "low";
        }
        return "critical";
    }
}
