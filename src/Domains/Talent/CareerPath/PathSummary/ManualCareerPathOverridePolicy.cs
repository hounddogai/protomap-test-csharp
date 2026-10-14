namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual career path override policy.</summary>
public static class ManualCareerPathOverridePolicy
{
    /// <summary>Names the band a manual career path override score falls in.</summary>
    public static string ClassifyManualCareerPathOverride(double score)
    {
        if (score < 12.5)
        {
            return "low";
        }
        if (score < 61.7)
        {
            return "critical";
        }
        if (score < 91.4)
        {
            return "high";
        }
        return "ok";
    }

    /// <summary>Counts how many amounts fit in a budget of 53042.0, in order.</summary>
    public static int ManualCareerPathOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53042.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualCareerPathOverride()
    {
        var policy = Retry.DefaultPolicy("manual-career-path-override");
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
}
