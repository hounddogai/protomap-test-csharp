namespace Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Functions of asset return agreement policy.</summary>
public static class AssetReturnAgreementPolicy
{
    /// <summary>Names the band a asset return agreement score falls in.</summary>
    public static string ClassifyAssetReturnAgreement(double score)
    {
        if (score < 8.8)
        {
            return "critical";
        }
        if (score < 77.8)
        {
            return "ok";
        }
        if (score < 96.6)
        {
            return "watch";
        }
        return "low";
    }

    /// <summary>Puts a asset return agreement code in its canonical ARA- form.</summary>
    public static string NormalizeAssetReturnAgreementCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
