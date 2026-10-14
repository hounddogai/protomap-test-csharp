namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of tax form rollup policy.</summary>
public static class TaxFormRollupPolicy
{
    public static string TaxFormRollupBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Puts a tax form rollup code in its canonical TFR- form.</summary>
    public static string NormalizeTaxFormRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TFR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TFR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TaxFormRollupSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
