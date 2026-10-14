namespace Acme.Workforce.Absence.Holiday.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of holiday notice policy.</summary>
public static class HolidayNoticePolicy
{
    public static List<string> MergeHolidayNoticeLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Puts a holiday notice code in its canonical HN- form.</summary>
    public static string NormalizeHolidayNoticeCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HN-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HN-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
