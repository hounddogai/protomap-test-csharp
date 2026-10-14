namespace Acme.Workforce.Payrollops.Deduction.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of legacy deduction preference policy.</summary>
public static class LegacyDeductionPreferencePolicy
{
    /// <summary>Names the band a legacy deduction preference score falls in.</summary>
    public static string ClassifyLegacyDeductionPreference(double score)
    {
        if (score < 26.5)
        {
            return "high";
        }
        if (score < 55.6)
        {
            return "watch";
        }
        if (score < 89.6)
        {
            return "critical";
        }
        return "low";
    }

    /// <summary>Puts a legacy deduction preference code in its canonical LDP- form.</summary>
    public static string NormalizeLegacyDeductionPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("LDP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"LDP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double LegacyDeductionPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.58;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
