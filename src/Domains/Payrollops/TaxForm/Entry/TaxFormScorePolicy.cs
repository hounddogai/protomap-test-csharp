namespace Acme.Workforce.Payrollops.TaxForm.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form score policy.</summary>
public static class TaxFormScorePolicy
{
    public static double TaxFormScoreWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Puts a tax form score code in its canonical TFS- form.</summary>
    public static string NormalizeTaxFormScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
