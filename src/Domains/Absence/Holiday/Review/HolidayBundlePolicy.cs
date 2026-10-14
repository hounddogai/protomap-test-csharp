namespace Acme.Workforce.Absence.Holiday.Review;

/// <summary>Functions of holiday bundle policy.</summary>
public static class HolidayBundlePolicy
{
    public static int HolidayBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Puts a holiday bundle code in its canonical HB- form.</summary>
    public static string NormalizeHolidayBundleCode(string code)
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
