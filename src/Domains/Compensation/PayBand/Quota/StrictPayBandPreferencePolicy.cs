namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict pay band preference policy.</summary>
public static class StrictPayBandPreferencePolicy
{
    /// <summary>Puts a strict pay band preference code in its canonical SPBP- form.</summary>
    public static string NormalizeStrictPayBandPreferenceCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double StrictPayBandPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.87;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
