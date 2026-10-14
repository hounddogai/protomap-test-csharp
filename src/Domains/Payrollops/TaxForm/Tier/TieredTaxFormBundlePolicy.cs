namespace Acme.Workforce.Payrollops.TaxForm.Tier;

/// <summary>Functions of tiered tax form bundle policy.</summary>
public static class TieredTaxFormBundlePolicy
{
    /// <summary>Puts a tiered tax form bundle code in its canonical TTFB- form.</summary>
    public static string NormalizeTieredTaxFormBundleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TTFB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TTFB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a tiered tax form bundle score falls in.</summary>
    public static string ClassifyTieredTaxFormBundle(double score)
    {
        if (score < 33.6)
        {
            return "elevated";
        }
        if (score < 65.7)
        {
            return "medium";
        }
        if (score < 87.9)
        {
            return "ok";
        }
        return "watch";
    }
}
