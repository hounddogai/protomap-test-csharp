namespace Acme.Workforce.Talent.Pool.PoolWindow;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of capped pool forecast policy.</summary>
public static class CappedPoolForecastPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 17624.0, in order.</summary>
    public static int CappedPoolForecastBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 17624.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a capped pool forecast score falls in.</summary>
    public static string ClassifyCappedPoolForecast(double score)
    {
        if (score < 38.9)
        {
            return "minimal";
        }
        if (score < 77.6)
        {
            return "high";
        }
        if (score < 85.7)
        {
            return "watch";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedPoolForecast()
    {
        var policy = Retry.DefaultPolicy("capped-pool-forecast");
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

    public static List<string> MergeCappedPoolForecastLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
