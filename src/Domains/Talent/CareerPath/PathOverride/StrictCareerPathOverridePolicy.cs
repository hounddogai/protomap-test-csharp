namespace Acme.Workforce.Talent.CareerPath.PathOverride;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict career path override policy.</summary>
public static class StrictCareerPathOverridePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 74159.0, in order.</summary>
    public static int StrictCareerPathOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 74159.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictCareerPathOverride()
    {
        var policy = Retry.DefaultPolicy("strict-career-path-override");
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

    /// <summary>Names the band a strict career path override score falls in.</summary>
    public static string ClassifyStrictCareerPathOverride(double score)
    {
        if (score < 12.9)
        {
            return "elevated";
        }
        if (score < 73.2)
        {
            return "low";
        }
        return "high";
    }
}
