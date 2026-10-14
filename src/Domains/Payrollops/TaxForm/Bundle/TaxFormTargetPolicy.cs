namespace Acme.Workforce.Payrollops.TaxForm.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form target policy.</summary>
public static class TaxFormTargetPolicy
{
    public static string TaxFormTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a tax form target score falls in.</summary>
    public static string ClassifyTaxFormTarget(double score)
    {
        if (score < 27.9)
        {
            return "minimal";
        }
        if (score < 39.1)
        {
            return "medium";
        }
        if (score < 42.2)
        {
            return "watch";
        }
        return "ok";
    }
}
