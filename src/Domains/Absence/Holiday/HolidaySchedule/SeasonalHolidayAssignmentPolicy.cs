namespace Acme.Workforce.Absence.Holiday.HolidaySchedule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal holiday assignment policy.</summary>
public static class SeasonalHolidayAssignmentPolicy
{
    public static List<string> MergeSeasonalHolidayAssignmentLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a seasonal holiday assignment code in its canonical SHA- form.</summary>
    public static string NormalizeSeasonalHolidayAssignmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SHA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SHA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
