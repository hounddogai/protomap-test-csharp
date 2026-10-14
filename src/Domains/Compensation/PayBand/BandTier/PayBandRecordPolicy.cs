namespace Acme.Workforce.Compensation.PayBand.BandTier;

/// <summary>Functions of pay band record policy.</summary>
public static class PayBandRecordPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 47594.0, in order.</summary>
    public static int PayBandRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47594.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a pay band record score falls in.</summary>
    public static string ClassifyPayBandRecord(double score)
    {
        if (score < 9.7)
        {
            return "elevated";
        }
        if (score < 65.6)
        {
            return "critical";
        }
        if (score < 87.9)
        {
            return "high";
        }
        if (score < 95.5)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Puts a pay band record code in its canonical PBR- form.</summary>
    public static string NormalizePayBandRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PBR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PBR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
