namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of holiday threshold policy.</summary>
public static class HolidayThresholdPolicy
{
    public static List<string> MergeHolidayThresholdLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int HolidayThresholdSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Puts a holiday threshold code in its canonical HT- form.</summary>
    public static string NormalizeHolidayThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a holiday threshold score falls in.</summary>
    public static string ClassifyHolidayThreshold(double score)
    {
        if (score < 58.8)
        {
            return "watch";
        }
        if (score < 63.7)
        {
            return "high";
        }
        return "elevated";
    }
}
