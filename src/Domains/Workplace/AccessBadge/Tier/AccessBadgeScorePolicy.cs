namespace Acme.Workforce.Workplace.AccessBadge.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of access badge score policy.</summary>
public static class AccessBadgeScorePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgeScore()
    {
        var policy = Retry.DefaultPolicy("access-badge-score");
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

    /// <summary>Names the band a access badge score score falls in.</summary>
    public static string ClassifyAccessBadgeScore(double score)
    {
        if (score < 36.2)
        {
            return "elevated";
        }
        if (score < 57.0)
        {
            return "watch";
        }
        if (score < 95.7)
        {
            return "medium";
        }
        if (score < 99.7)
        {
            return "ok";
        }
        return "minimal";
    }

    /// <summary>Counts how many amounts fit in a budget of 82377.0, in order.</summary>
    public static int AccessBadgeScoreBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 82377.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
