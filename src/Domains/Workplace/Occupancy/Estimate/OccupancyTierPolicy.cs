namespace Acme.Workforce.Workplace.Occupancy.Estimate;

/// <summary>Functions of occupancy tier policy.</summary>
public static class OccupancyTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 65094.0, in order.</summary>
    public static int OccupancyTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 65094.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a occupancy tier code in its canonical OT- form.</summary>
    public static string NormalizeOccupancyTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
