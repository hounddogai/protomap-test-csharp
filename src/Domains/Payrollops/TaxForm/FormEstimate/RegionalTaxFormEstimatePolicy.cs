namespace Acme.Workforce.Payrollops.TaxForm.FormEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of regional tax form estimate policy.</summary>
public static class RegionalTaxFormEstimatePolicy
{
    public static double RegionalTaxFormEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.06;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a regional tax form estimate code in its canonical RTFE- form.</summary>
    public static string NormalizeRegionalTaxFormEstimateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RTFE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RTFE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a regional tax form estimate score falls in.</summary>
    public static string ClassifyRegionalTaxFormEstimate(double score)
    {
        if (score < 5.9)
        {
            return "watch";
        }
        if (score < 51.6)
        {
            return "ok";
        }
        if (score < 55.2)
        {
            return "medium";
        }
        if (score < 73.2)
        {
            return "critical";
        }
        return "low";
    }
}
