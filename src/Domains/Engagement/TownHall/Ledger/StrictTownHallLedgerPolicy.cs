namespace Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Functions of strict town hall ledger policy.</summary>
public static class StrictTownHallLedgerPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 87348.0, in order.</summary>
    public static int StrictTownHallLedgerBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 87348.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a strict town hall ledger code in its canonical STHL- form.</summary>
    public static string NormalizeStrictTownHallLedgerCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("STHL-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"STHL-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
