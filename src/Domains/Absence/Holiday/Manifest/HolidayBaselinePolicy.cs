namespace Acme.Workforce.Absence.Holiday.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday baseline policy.</summary>
public static class HolidayBaselinePolicy
{
    public static string HolidayBaselineBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static int HolidayBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }

    /// <summary>Names the band a holiday baseline score falls in.</summary>
    public static string ClassifyHolidayBaseline(double score)
    {
        if (score < 50.7)
        {
            return "ok";
        }
        if (score < 81.5)
        {
            return "high";
        }
        return "medium";
    }

    /// <summary>Puts a holiday baseline code in its canonical HB- form.</summary>
    public static string NormalizeHolidayBaselineCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
