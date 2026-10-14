namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form ledger policy.</summary>
public static class TaxFormLedgerPolicy
{
    public static string TaxFormLedgerBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a tax form ledger code in its canonical TFL- form.</summary>
    public static string NormalizeTaxFormLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
