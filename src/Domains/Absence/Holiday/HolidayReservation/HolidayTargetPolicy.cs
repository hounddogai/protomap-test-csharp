namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of holiday target policy.</summary>
public static class HolidayTargetPolicy
{
    public static List<string> MergeHolidayTargetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int HolidayTargetSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a holiday target score falls in.</summary>
    public static string ClassifyHolidayTarget(double score)
    {
        if (score < 32.5)
        {
            return "critical";
        }
        if (score < 79.1)
        {
            return "ok";
        }
        return "medium";
    }

    /// <summary>Puts a holiday target code in its canonical HT- form.</summary>
    public static string NormalizeHolidayTargetCode(string code)
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
}
