namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of access badge review policy.</summary>
public static class AccessBadgeReviewPolicy
{
    /// <summary>Names the band a access badge review score falls in.</summary>
    public static string ClassifyAccessBadgeReview(double score)
    {
        if (score < 16.5)
        {
            return "critical";
        }
        if (score < 48.1)
        {
            return "medium";
        }
        if (score < 82.3)
        {
            return "elevated";
        }
        if (score < 91.2)
        {
            return "low";
        }
        return "high";
    }

    public static string AccessBadgeReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Counts how many amounts fit in a budget of 31171.0, in order.</summary>
    public static int AccessBadgeReviewBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31171.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAccessBadgeReview()
    {
        var policy = Retry.DefaultPolicy("access-badge-review");
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
}
