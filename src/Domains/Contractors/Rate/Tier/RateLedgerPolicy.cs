namespace Acme.Workforce.Contractors.Rate.Tier;

/// <summary>Functions of rate ledger policy.</summary>
public static class RateLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 51676.0, in order.</summary>
    public static int RateLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 51676.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a rate ledger code in its canonical RL- form.</summary>
    public static string NormalizeRateLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
