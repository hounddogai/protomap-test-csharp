namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

/// <summary>Functions of seasonal tax form allocation policy.</summary>
public static class SeasonalTaxFormAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 48261.0, in order.</summary>
    public static int SeasonalTaxFormAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48261.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a seasonal tax form allocation code in its canonical STFA- form.</summary>
    public static string NormalizeSeasonalTaxFormAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("STFA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"STFA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int SeasonalTaxFormAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Names the band a seasonal tax form allocation score falls in.</summary>
    public static string ClassifySeasonalTaxFormAllocation(double score)
    {
        if (score < 40.3)
        {
            return "low";
        }
        if (score < 54.6)
        {
            return "ok";
        }
        if (score < 97.9)
        {
            return "elevated";
        }
        return "high";
    }
}
