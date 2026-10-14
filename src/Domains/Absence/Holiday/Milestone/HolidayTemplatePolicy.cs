namespace Acme.Workforce.Absence.Holiday.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday template policy.</summary>
public static class HolidayTemplatePolicy
{
    public static int HolidayTemplateSpan(int start, int end)
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
        return span / 86400;
    }

    public static string HolidayTemplateBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Puts a holiday template code in its canonical HT- form.</summary>
    public static string NormalizeHolidayTemplateCode(string code)
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
