namespace Acme.Workforce.Absence.Accrual.Baseline;

/// <summary>Functions of pending accrual window policy.</summary>
public static class PendingAccrualWindowPolicy
{
    /// <summary>Puts a pending accrual window code in its canonical PAW- form.</summary>
    public static string NormalizePendingAccrualWindowCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAW-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAW-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 53134.0, in order.</summary>
    public static int PendingAccrualWindowBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53134.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
