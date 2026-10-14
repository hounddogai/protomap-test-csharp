namespace Acme.Workforce.Engagement.TownHall.Ledger;

/// <summary>Functions of regional town hall mapping policy.</summary>
public static class RegionalTownHallMappingPolicy
{
    /// <summary>Names the band a regional town hall mapping score falls in.</summary>
    public static string ClassifyRegionalTownHallMapping(double score)
    {
        if (score < 35.4)
        {
            return "medium";
        }
        if (score < 99.2)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 50080.0, in order.</summary>
    public static int RegionalTownHallMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50080.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
