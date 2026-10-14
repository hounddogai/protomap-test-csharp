namespace Acme.Workforce.Payrollops.TaxForm.FormMetric;

/// <summary>Functions of fallback tax form metric policy.</summary>
public static class FallbackTaxFormMetricPolicy
{
    public static int FallbackTaxFormMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Puts a fallback tax form metric code in its canonical FTFM- form.</summary>
    public static string NormalizeFallbackTaxFormMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("FTFM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"FTFM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 1086.0, in order.</summary>
    public static int FallbackTaxFormMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 1086.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
