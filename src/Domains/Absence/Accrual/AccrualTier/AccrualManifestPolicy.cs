namespace Acme.Workforce.Absence.Accrual.AccrualTier;

/// <summary>Functions of accrual manifest policy.</summary>
public static class AccrualManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 31757.0, in order.</summary>
    public static int AccrualManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31757.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a accrual manifest code in its canonical AM- form.</summary>
    public static string NormalizeAccrualManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a accrual manifest score falls in.</summary>
    public static string ClassifyAccrualManifest(double score)
    {
        if (score < 10.1)
        {
            return "watch";
        }
        if (score < 38.6)
        {
            return "critical";
        }
        if (score < 86.1)
        {
            return "high";
        }
        return "medium";
    }
}
