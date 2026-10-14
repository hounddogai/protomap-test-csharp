namespace Acme.Workforce.Engagement.TownHall.Cycle;

/// <summary>Functions of batched town hall quota policy.</summary>
public static class BatchedTownHallQuotaPolicy
{
    /// <summary>Names the band a batched town hall quota score falls in.</summary>
    public static string ClassifyBatchedTownHallQuota(double score)
    {
        if (score < 13.4)
        {
            return "minimal";
        }
        if (score < 39.3)
        {
            return "elevated";
        }
        if (score < 71.8)
        {
            return "ok";
        }
        if (score < 87.7)
        {
            return "medium";
        }
        return "low";
    }

    /// <summary>Counts how many amounts fit in a budget of 76324.0, in order.</summary>
    public static int BatchedTownHallQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 76324.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
