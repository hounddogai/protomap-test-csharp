namespace Acme.Workforce.Contractors.Engagement.Assignment;

/// <summary>Functions of engagement ledger policy.</summary>
public static class EngagementLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 47738.0, in order.</summary>
    public static int EngagementLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 47738.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a engagement ledger code in its canonical EL- form.</summary>
    public static string NormalizeEngagementLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("EL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"EL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
