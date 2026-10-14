namespace Acme.Workforce.Engagement.TownHall.Milestone;

/// <summary>Functions of town hall manifest policy.</summary>
public static class TownHallManifestPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 95995.0, in order.</summary>
    public static int TownHallManifestBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 95995.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a town hall manifest code in its canonical THM- form.</summary>
    public static string NormalizeTownHallManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("THM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"THM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
