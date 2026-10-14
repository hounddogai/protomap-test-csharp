namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

/// <summary>Functions of pending equity grant override policy.</summary>
public static class PendingEquityGrantOverridePolicy
{
    /// <summary>Puts a pending equity grant override code in its canonical PEGO- form.</summary>
    public static string NormalizePendingEquityGrantOverrideCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PEGO-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PEGO-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 80176.0, in order.</summary>
    public static int PendingEquityGrantOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 80176.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
