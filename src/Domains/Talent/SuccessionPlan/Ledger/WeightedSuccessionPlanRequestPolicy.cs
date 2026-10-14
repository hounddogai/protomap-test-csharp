namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of weighted succession plan request policy.</summary>
public static class WeightedSuccessionPlanRequestPolicy
{
    public static double WeightedSuccessionPlanRequestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.71;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a weighted succession plan request score falls in.</summary>
    public static string ClassifyWeightedSuccessionPlanRequest(double score)
    {
        if (score < 10.4)
        {
            return "medium";
        }
        if (score < 13.0)
        {
            return "low";
        }
        if (score < 24.6)
        {
            return "elevated";
        }
        return "ok";
    }
}
