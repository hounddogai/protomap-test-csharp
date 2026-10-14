namespace Acme.Workforce.Workplace.Visitor.Estimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of visitor estimate policy.</summary>
public static class VisitorEstimatePolicy
{
    /// <summary>Names the band a visitor estimate score falls in.</summary>
    public static string ClassifyVisitorEstimate(double score)
    {
        if (score < 11.1)
        {
            return "low";
        }
        if (score < 11.1)
        {
            return "elevated";
        }
        if (score < 83.0)
        {
            return "medium";
        }
        if (score < 84.0)
        {
            return "watch";
        }
        return "high";
    }

    public static double VisitorEstimateWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.73;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
