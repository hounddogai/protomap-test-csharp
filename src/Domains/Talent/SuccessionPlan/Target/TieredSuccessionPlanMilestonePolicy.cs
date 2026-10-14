namespace Acme.Workforce.Talent.SuccessionPlan.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of tiered succession plan milestone policy.</summary>
public static class TieredSuccessionPlanMilestonePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 42322.0, in order.</summary>
    public static int TieredSuccessionPlanMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 42322.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static List<string> MergeTieredSuccessionPlanMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int TieredSuccessionPlanMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Names the band a tiered succession plan milestone score falls in.</summary>
    public static string ClassifyTieredSuccessionPlanMilestone(double score)
    {
        if (score < 3.2)
        {
            return "minimal";
        }
        if (score < 57.3)
        {
            return "elevated";
        }
        if (score < 63.8)
        {
            return "critical";
        }
        return "medium";
    }
}
