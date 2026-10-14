namespace Acme.Workforce.Absence.Accrual.AccrualAssignment;

/// <summary>Functions of weighted accrual bundle policy.</summary>
public static class WeightedAccrualBundlePolicy
{
    public static int WeightedAccrualBundleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Counts how many amounts fit in a budget of 8902.0, in order.</summary>
    public static int WeightedAccrualBundleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 8902.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a weighted accrual bundle score falls in.</summary>
    public static string ClassifyWeightedAccrualBundle(double score)
    {
        if (score < 30.1)
        {
            return "medium";
        }
        if (score < 81.2)
        {
            return "critical";
        }
        if (score < 97.9)
        {
            return "high";
        }
        return "watch";
    }
}
