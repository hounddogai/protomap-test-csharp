namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional career path rollup policy.</summary>
public static class RegionalCareerPathRollupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalCareerPathRollup()
    {
        var policy = Retry.DefaultPolicy("regional-career-path-rollup");
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

    /// <summary>Counts how many amounts fit in a budget of 64764.0, in order.</summary>
    public static int RegionalCareerPathRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 64764.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a regional career path rollup score falls in.</summary>
    public static string ClassifyRegionalCareerPathRollup(double score)
    {
        if (score < 23.8)
        {
            return "low";
        }
        if (score < 57.5)
        {
            return "ok";
        }
        return "high";
    }
}
