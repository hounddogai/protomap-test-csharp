namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of equity grant override policy.</summary>
public static class EquityGrantOverridePolicy
{
    /// <summary>Counts how many amounts fit in a budget of 25357.0, in order.</summary>
    public static int EquityGrantOverrideBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 25357.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a equity grant override score falls in.</summary>
    public static string ClassifyEquityGrantOverride(double score)
    {
        if (score < 40.4)
        {
            return "watch";
        }
        if (score < 55.1)
        {
            return "elevated";
        }
        if (score < 88.4)
        {
            return "low";
        }
        return "ok";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeEquityGrantOverride()
    {
        var policy = Retry.DefaultPolicy("equity-grant-override");
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

    public static int EquityGrantOverrideSpan(int start, int end)
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
        return span / 86400;
    }
}
