namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of succession plan mapping policy.</summary>
public static class SuccessionPlanMappingPolicy
{
    /// <summary>Names the band a succession plan mapping score falls in.</summary>
    public static string ClassifySuccessionPlanMapping(double score)
    {
        if (score < 58.4)
        {
            return "elevated";
        }
        if (score < 83.8)
        {
            return "medium";
        }
        return "minimal";
    }

    public static double SuccessionPlanMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.53;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 52831.0, in order.</summary>
    public static int SuccessionPlanMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 52831.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static string SuccessionPlanMappingBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
