namespace Acme.Workforce.Absence.Balance.Summary;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted balance summary policy.</summary>
public static class WeightedBalanceSummaryPolicy
{
    /// <summary>Names the band a weighted balance summary score falls in.</summary>
    public static string ClassifyWeightedBalanceSummary(double score)
    {
        if (score < 2.6)
        {
            return "medium";
        }
        if (score < 17.7)
        {
            return "critical";
        }
        if (score < 57.0)
        {
            return "watch";
        }
        if (score < 68.9)
        {
            return "high";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedBalanceSummary()
    {
        var policy = Retry.DefaultPolicy("weighted-balance-summary");
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
