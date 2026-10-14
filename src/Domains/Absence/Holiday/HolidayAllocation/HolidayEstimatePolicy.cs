namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of holiday estimate policy.</summary>
public static class HolidayEstimatePolicy
{
    public static List<string> MergeHolidayEstimateLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int HolidayEstimateSpan(int start, int end)
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

    /// <summary>Names the band a holiday estimate score falls in.</summary>
    public static string ClassifyHolidayEstimate(double score)
    {
        if (score < 21.0)
        {
            return "low";
        }
        if (score < 58.0)
        {
            return "watch";
        }
        return "minimal";
    }
}
