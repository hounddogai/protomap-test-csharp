namespace Acme.Workforce.Talent.SuccessionPlan.Policy;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy succession plan policy policy.</summary>
public static class LegacySuccessionPlanPolicyPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 10686.0, in order.</summary>
    public static int LegacySuccessionPlanPolicyBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 10686.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a legacy succession plan policy score falls in.</summary>
    public static string ClassifyLegacySuccessionPlanPolicy(double score)
    {
        if (score < 67.9)
        {
            return "high";
        }
        if (score < 75.3)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacySuccessionPlanPolicy()
    {
        var policy = Retry.DefaultPolicy("legacy-succession-plan-policy");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
