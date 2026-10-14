namespace Acme.Workforce.Payrollops.TaxForm.Score;

/// <summary>Functions of adaptive tax form score policy.</summary>
public static class AdaptiveTaxFormScorePolicy
{
    /// <summary>Puts a adaptive tax form score code in its canonical ATFS- form.</summary>
    public static string NormalizeAdaptiveTaxFormScoreCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ATFS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ATFS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a adaptive tax form score score falls in.</summary>
    public static string ClassifyAdaptiveTaxFormScore(double score)
    {
        if (score < 21.3)
        {
            return "ok";
        }
        if (score < 89.8)
        {
            return "elevated";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 33059.0, in order.</summary>
    public static int AdaptiveTaxFormScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 33059.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
