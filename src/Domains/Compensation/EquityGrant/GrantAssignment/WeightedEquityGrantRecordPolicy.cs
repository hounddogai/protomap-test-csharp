namespace Acme.Workforce.Compensation.EquityGrant.GrantAssignment;

/// <summary>Functions of weighted equity grant record policy.</summary>
public static class WeightedEquityGrantRecordPolicy
{
    /// <summary>Names the band a weighted equity grant record score falls in.</summary>
    public static string ClassifyWeightedEquityGrantRecord(double score)
    {
        if (score < 17.5)
        {
            return "watch";
        }
        if (score < 45.5)
        {
            return "elevated";
        }
        if (score < 59.9)
        {
            return "critical";
        }
        if (score < 88.5)
        {
            return "medium";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 50855.0, in order.</summary>
    public static int WeightedEquityGrantRecordBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 50855.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int WeightedEquityGrantRecordSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }
}
