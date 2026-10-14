namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of equity grant summary policy.</summary>
public static class EquityGrantSummaryPolicy
{
    public static double EquityGrantSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.54;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a equity grant summary score falls in.</summary>
    public static string ClassifyEquityGrantSummary(double score)
    {
        if (score < 2.7)
        {
            return "medium";
        }
        if (score < 38.1)
        {
            return "ok";
        }
        return "low";
    }
}
