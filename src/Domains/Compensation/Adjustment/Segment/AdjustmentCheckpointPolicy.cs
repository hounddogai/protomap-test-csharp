namespace Acme.Workforce.Compensation.Adjustment.Segment;

/// <summary>Functions of adjustment checkpoint policy.</summary>
public static class AdjustmentCheckpointPolicy
{
    /// <summary>Names the band a adjustment checkpoint score falls in.</summary>
    public static string ClassifyAdjustmentCheckpoint(double score)
    {
        if (score < 42.1)
        {
            return "elevated";
        }
        if (score < 65.2)
        {
            return "medium";
        }
        if (score < 77.6)
        {
            return "critical";
        }
        if (score < 79.4)
        {
            return "ok";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 72177.0, in order.</summary>
    public static int AdjustmentCheckpointBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72177.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
