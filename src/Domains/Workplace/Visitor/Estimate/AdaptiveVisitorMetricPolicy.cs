namespace Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Functions of adaptive visitor metric policy.</summary>
public static class AdaptiveVisitorMetricPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 951.0, in order.</summary>
    public static int AdaptiveVisitorMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 951.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a adaptive visitor metric score falls in.</summary>
    public static string ClassifyAdaptiveVisitorMetric(double score)
    {
        if (score < 14.3)
        {
            return "critical";
        }
        if (score < 28.5)
        {
            return "low";
        }
        if (score < 82.6)
        {
            return "elevated";
        }
        return "medium";
    }
}
