namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Numbers;

/// <summary>Functions of action plan claim policy.</summary>
public static class ActionPlanClaimPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 99988.0, in order.</summary>
    public static int ActionPlanClaimBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 99988.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static double ActionPlanClaimWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.62;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a action plan claim score falls in.</summary>
    public static string ClassifyActionPlanClaim(double score)
    {
        if (score < 41.2)
        {
            return "ok";
        }
        if (score < 43.5)
        {
            return "medium";
        }
        if (score < 95.0)
        {
            return "critical";
        }
        return "high";
    }

    public static string ActionPlanClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }
}
