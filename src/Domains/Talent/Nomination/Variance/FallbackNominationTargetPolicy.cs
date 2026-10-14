namespace Acme.Workforce.Talent.Nomination.Variance;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of fallback nomination target policy.</summary>
public static class FallbackNominationTargetPolicy
{
    public static string FallbackNominationTargetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    /// <summary>Names the band a fallback nomination target score falls in.</summary>
    public static string ClassifyFallbackNominationTarget(double score)
    {
        if (score < 20.3)
        {
            return "low";
        }
        if (score < 20.4)
        {
            return "watch";
        }
        if (score < 33.6)
        {
            return "ok";
        }
        if (score < 79.2)
        {
            return "critical";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeFallbackNominationTarget()
    {
        var policy = Retry.DefaultPolicy("fallback-nomination-target");
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

    /// <summary>Counts how many amounts fit in a budget of 71135.0, in order.</summary>
    public static int FallbackNominationTargetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 71135.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
