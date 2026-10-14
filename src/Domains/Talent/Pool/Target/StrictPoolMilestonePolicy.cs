namespace Acme.Workforce.Talent.Pool.Target;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of strict pool milestone policy.</summary>
public static class StrictPoolMilestonePolicy
{
    /// <summary>Names the band a strict pool milestone score falls in.</summary>
    public static string ClassifyStrictPoolMilestone(double score)
    {
        if (score < 23.0)
        {
            return "elevated";
        }
        if (score < 40.8)
        {
            return "high";
        }
        if (score < 61.0)
        {
            return "critical";
        }
        return "medium";
    }

    public static double StrictPoolMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Counts how many amounts fit in a budget of 5263.0, in order.</summary>
    public static int StrictPoolMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5263.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
