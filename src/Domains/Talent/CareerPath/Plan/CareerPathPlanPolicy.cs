namespace Acme.Workforce.Talent.CareerPath.Plan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of career path plan policy.</summary>
public static class CareerPathPlanPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 55790.0, in order.</summary>
    public static int CareerPathPlanBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 55790.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a career path plan score falls in.</summary>
    public static string ClassifyCareerPathPlan(double score)
    {
        if (score < 39.3)
        {
            return "watch";
        }
        if (score < 61.0)
        {
            return "high";
        }
        if (score < 64.4)
        {
            return "low";
        }
        if (score < 94.6)
        {
            return "minimal";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCareerPathPlan()
    {
        var policy = Retry.DefaultPolicy("career-path-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
