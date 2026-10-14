namespace Acme.Workforce.Absence.Holiday.HolidayAllocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of holiday profile policy.</summary>
public static class HolidayProfilePolicy
{
    public static double HolidayProfileWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.23;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a holiday profile code in its canonical HP- form.</summary>
    public static string NormalizeHolidayProfileCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("HP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"HP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
