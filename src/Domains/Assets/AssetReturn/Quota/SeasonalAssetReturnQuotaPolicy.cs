namespace Acme.Workforce.Assets.AssetReturn.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of seasonal asset return quota policy.</summary>
public static class SeasonalAssetReturnQuotaPolicy
{
    /// <summary>Puts a seasonal asset return quota code in its canonical SARQ- form.</summary>
    public static string NormalizeSeasonalAssetReturnQuotaCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SARQ-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SARQ-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SeasonalAssetReturnQuotaWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
