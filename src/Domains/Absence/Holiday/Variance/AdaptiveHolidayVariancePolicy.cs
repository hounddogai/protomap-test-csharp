namespace Acme.Workforce.Absence.Holiday.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive holiday variance policy.</summary>
public static class AdaptiveHolidayVariancePolicy
{
    public static double AdaptiveHolidayVarianceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.44;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a adaptive holiday variance code in its canonical AHV- form.</summary>
    public static string NormalizeAdaptiveHolidayVarianceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AHV-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AHV-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
