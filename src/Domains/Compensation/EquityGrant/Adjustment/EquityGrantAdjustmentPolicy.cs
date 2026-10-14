namespace Acme.Workforce.Compensation.EquityGrant.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of equity grant adjustment policy.</summary>
public static class EquityGrantAdjustmentPolicy
{
    /// <summary>Names the band a equity grant adjustment score falls in.</summary>
    public static string ClassifyEquityGrantAdjustment(double score)
    {
        if (score < 3.4)
        {
            return "critical";
        }
        if (score < 9.0)
        {
            return "ok";
        }
        if (score < 39.3)
        {
            return "medium";
        }
        if (score < 69.0)
        {
            return "low";
        }
        return "minimal";
    }

    public static int EquityGrantAdjustmentSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Counts how many amounts fit in a budget of 53261.0, in order.</summary>
    public static int EquityGrantAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 53261.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEquityGrantAdjustment()
    {
        var policy = Retry.DefaultPolicy("equity-grant-adjustment");
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
