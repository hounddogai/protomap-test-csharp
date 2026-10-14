namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of bonus pool milestone policy.</summary>
public static class BonusPoolMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 88172.0, in order.</summary>
    public static int BonusPoolMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 88172.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeBonusPoolMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a bonus pool milestone score falls in.</summary>
    public static string ClassifyBonusPoolMilestone(double score)
    {
        if (score < 65.5)
        {
            return "minimal";
        }
        if (score < 81.0)
        {
            return "medium";
        }
        if (score < 92.2)
        {
            return "critical";
        }
        if (score < 94.1)
        {
            return "watch";
        }
        return "high";
    }

    public static double BonusPoolMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.04;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
