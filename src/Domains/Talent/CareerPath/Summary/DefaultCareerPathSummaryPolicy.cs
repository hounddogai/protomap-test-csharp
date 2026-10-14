namespace Acme.Workforce.Talent.CareerPath.Summary;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default career path summary policy.</summary>
public static class DefaultCareerPathSummaryPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultCareerPathSummary()
    {
        var policy = Retry.DefaultPolicy("default-career-path-summary");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static string DefaultCareerPathSummaryBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Names the band a default career path summary score falls in.</summary>
    public static string ClassifyDefaultCareerPathSummary(double score)
    {
        if (score < 7.8)
        {
            return "low";
        }
        if (score < 8.9)
        {
            return "elevated";
        }
        if (score < 17.7)
        {
            return "high";
        }
        if (score < 47.5)
        {
            return "critical";
        }
        return "medium";
    }

    /// <summary>Counts how many amounts fit in a budget of 36026.0, in order.</summary>
    public static int DefaultCareerPathSummaryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 36026.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
