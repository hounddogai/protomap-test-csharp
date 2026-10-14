namespace Acme.Workforce.Talent.CareerPath.GlobalPathRule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of adaptive career path rule policy.</summary>
public static class AdaptiveCareerPathRulePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdaptiveCareerPathRule()
    {
        var policy = Retry.DefaultPolicy("adaptive-career-path-rule");
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

    /// <summary>Counts how many amounts fit in a budget of 7028.0, in order.</summary>
    public static int AdaptiveCareerPathRuleBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 7028.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a adaptive career path rule score falls in.</summary>
    public static string ClassifyAdaptiveCareerPathRule(double score)
    {
        if (score < 19.4)
        {
            return "elevated";
        }
        if (score < 95.4)
        {
            return "high";
        }
        return "minimal";
    }
}
