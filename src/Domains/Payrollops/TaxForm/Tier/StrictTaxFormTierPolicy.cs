namespace Acme.Workforce.Payrollops.TaxForm.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict tax form tier policy.</summary>
public static class StrictTaxFormTierPolicy
{
    /// <summary>Names the band a strict tax form tier score falls in.</summary>
    public static string ClassifyStrictTaxFormTier(double score)
    {
        if (score < 6.0)
        {
            return "elevated";
        }
        if (score < 13.6)
        {
            return "ok";
        }
        if (score < 90.0)
        {
            return "critical";
        }
        if (score < 93.4)
        {
            return "low";
        }
        return "watch";
    }

    public static string StrictTaxFormTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }
}
