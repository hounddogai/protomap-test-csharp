namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending town hall plan policy.</summary>
public static class PendingTownHallPlanPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingTownHallPlan()
    {
        var policy = Retry.DefaultPolicy("pending-town-hall-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a pending town hall plan score falls in.</summary>
    public static string ClassifyPendingTownHallPlan(double score)
    {
        if (score < 26.0)
        {
            return "low";
        }
        if (score < 39.6)
        {
            return "high";
        }
        if (score < 56.6)
        {
            return "ok";
        }
        if (score < 70.0)
        {
            return "minimal";
        }
        return "medium";
    }

    public static string PendingTownHallPlanBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 5138.0, in order.</summary>
    public static int PendingTownHallPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 5138.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
