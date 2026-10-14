namespace Acme.Workforce.Compensation.EquityGrant.OverrideItem;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary equity grant override policy.</summary>
public static class PrimaryEquityGrantOverridePolicy
{
    /// <summary>Names the band a primary equity grant override score falls in.</summary>
    public static string ClassifyPrimaryEquityGrantOverride(double score)
    {
        if (score < 1.8)
        {
            return "minimal";
        }
        if (score < 7.7)
        {
            return "ok";
        }
        if (score < 35.3)
        {
            return "high";
        }
        if (score < 53.0)
        {
            return "elevated";
        }
        return "watch";
    }

    /// <summary>Counts how many amounts fit in a budget of 98086.0, in order.</summary>
    public static int PrimaryEquityGrantOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 98086.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PrimaryEquityGrantOverrideSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryEquityGrantOverride()
    {
        var policy = Retry.DefaultPolicy("primary-equity-grant-override");
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
