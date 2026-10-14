namespace Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Functions of warranty summary policy.</summary>
public static class WarrantySummaryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 302.0, in order.</summary>
    public static int WarrantySummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 302.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a warranty summary code in its canonical WS- form.</summary>
    public static string NormalizeWarrantySummaryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
