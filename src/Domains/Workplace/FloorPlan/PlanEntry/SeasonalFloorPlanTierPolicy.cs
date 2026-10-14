namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

/// <summary>Functions of seasonal floor plan tier policy.</summary>
public static class SeasonalFloorPlanTierPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 37924.0, in order.</summary>
    public static int SeasonalFloorPlanTierBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37924.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a seasonal floor plan tier code in its canonical SFPT- form.</summary>
    public static string NormalizeSeasonalFloorPlanTierCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SFPT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SFPT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
