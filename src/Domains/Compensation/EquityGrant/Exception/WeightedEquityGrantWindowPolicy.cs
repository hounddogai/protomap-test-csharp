namespace Acme.Workforce.Compensation.EquityGrant.Exception;

/// <summary>Functions of weighted equity grant window policy.</summary>
public static class WeightedEquityGrantWindowPolicy
{
    public static int WeightedEquityGrantWindowSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Names the band a weighted equity grant window score falls in.</summary>
    public static string ClassifyWeightedEquityGrantWindow(double score)
    {
        if (score < 9.0)
        {
            return "medium";
        }
        if (score < 17.3)
        {
            return "elevated";
        }
        if (score < 31.9)
        {
            return "ok";
        }
        if (score < 49.3)
        {
            return "low";
        }
        return "critical";
    }
}
