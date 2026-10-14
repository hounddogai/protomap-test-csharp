namespace Acme.Workforce.Payrollops.TaxForm.Slot;

/// <summary>Functions of tax form cycle policy.</summary>
public static class TaxFormCyclePolicy
{
    /// <summary>Names the band a tax form cycle score falls in.</summary>
    public static string ClassifyTaxFormCycle(double score)
    {
        if (score < 43.1)
        {
            return "ok";
        }
        if (score < 48.8)
        {
            return "high";
        }
        if (score < 75.8)
        {
            return "minimal";
        }
        return "low";
    }

    /// <summary>Puts a tax form cycle code in its canonical TFC- form.</summary>
    public static string NormalizeTaxFormCycleCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
