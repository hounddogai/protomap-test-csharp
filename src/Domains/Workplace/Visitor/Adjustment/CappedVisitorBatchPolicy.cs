namespace Acme.Workforce.Workplace.Visitor.Adjustment;

/// <summary>Functions of capped visitor batch policy.</summary>
public static class CappedVisitorBatchPolicy
{
    /// <summary>Names the band a capped visitor batch score falls in.</summary>
    public static string ClassifyCappedVisitorBatch(double score)
    {
        if (score < 38.3)
        {
            return "elevated";
        }
        if (score < 62.5)
        {
            return "high";
        }
        if (score < 66.8)
        {
            return "watch";
        }
        if (score < 69.0)
        {
            return "medium";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 90099.0, in order.</summary>
    public static int CappedVisitorBatchBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 90099.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
