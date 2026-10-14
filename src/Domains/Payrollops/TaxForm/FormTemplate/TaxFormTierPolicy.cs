namespace Acme.Workforce.Payrollops.TaxForm.FormTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form tier policy.</summary>
public static class TaxFormTierPolicy
{
    /// <summary>Puts a tax form tier code in its canonical TFT- form.</summary>
    public static string NormalizeTaxFormTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TaxFormTierSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 60;
    }

    public static string TaxFormTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
