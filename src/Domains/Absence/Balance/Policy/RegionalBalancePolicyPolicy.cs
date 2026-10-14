namespace Acme.Workforce.Absence.Balance.Policy;

/// <summary>Functions of regional balance policy policy.</summary>
public static class RegionalBalancePolicyPolicy
{
    /// <summary>Puts a regional balance policy code in its canonical RBP- form.</summary>
    public static string NormalizeRegionalBalancePolicyCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RBP-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RBP-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 85076.0, in order.</summary>
    public static int RegionalBalancePolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 85076.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
