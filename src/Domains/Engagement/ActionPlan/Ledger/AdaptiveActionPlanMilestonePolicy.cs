namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of adaptive action plan milestone policy.</summary>
public static class AdaptiveActionPlanMilestonePolicy
{
    /// <summary>Names the band a adaptive action plan milestone score falls in.</summary>
    public static string ClassifyAdaptiveActionPlanMilestone(double score)
    {
        if (score < 20.6)
        {
            return "medium";
        }
        if (score < 25.6)
        {
            return "high";
        }
        if (score < 66.8)
        {
            return "ok";
        }
        if (score < 74.1)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 82350.0, in order.</summary>
    public static int AdaptiveActionPlanMilestoneBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82350.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int AdaptiveActionPlanMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    public static double AdaptiveActionPlanMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.86;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
