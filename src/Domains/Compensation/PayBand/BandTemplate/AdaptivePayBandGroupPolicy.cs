namespace Acme.Workforce.Compensation.PayBand.BandTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive pay band group policy.</summary>
public static class AdaptivePayBandGroupPolicy
{
    /// <summary>Puts a adaptive pay band group code in its canonical APBG- form.</summary>
    public static string NormalizeAdaptivePayBandGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("APBG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"APBG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AdaptivePayBandGroupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.45;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
