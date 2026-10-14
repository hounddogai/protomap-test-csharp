namespace Acme.Workforce.Absence.Holiday.Budget;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of holiday entry policy.</summary>
public static class HolidayEntryPolicy
{
    /// <summary>Names the band a holiday entry score falls in.</summary>
    public static string ClassifyHolidayEntry(double score)
    {
        if (score < 46.9)
        {
            return "medium";
        }
        if (score < 89.0)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Puts a holiday entry code in its canonical HE- form.</summary>
    public static string NormalizeHolidayEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeHolidayEntryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static string HolidayEntryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
