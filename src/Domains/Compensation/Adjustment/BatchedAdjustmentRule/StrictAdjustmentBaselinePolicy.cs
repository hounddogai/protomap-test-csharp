namespace Acme.Workforce.Compensation.Adjustment.BatchedAdjustmentRule;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict adjustment baseline policy.</summary>
public static class StrictAdjustmentBaselinePolicy
{
    /// <summary>Names the band a strict adjustment baseline score falls in.</summary>
    public static string ClassifyStrictAdjustmentBaseline(double score)
    {
        if (score < 3.2)
        {
            return "watch";
        }
        if (score < 17.7)
        {
            return "medium";
        }
        if (score < 26.2)
        {
            return "low";
        }
        if (score < 54.4)
        {
            return "critical";
        }
        return "high";
    }

    /// <summary>Counts how many amounts fit in a budget of 44714.0, in order.</summary>
    public static int StrictAdjustmentBaselineBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 44714.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int StrictAdjustmentBaselineSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictAdjustmentBaseline()
    {
        var policy = Retry.DefaultPolicy("strict-adjustment-baseline");
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
