namespace Acme.Workforce.Absence.Balance.Threshold;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary balance metric policy.</summary>
public static class PrimaryBalanceMetricPolicy
{
    public static string PrimaryBalanceMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryBalanceMetric()
    {
        var policy = Retry.DefaultPolicy("primary-balance-metric");
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

    /// <summary>Counts how many amounts fit in a budget of 26249.0, in order.</summary>
    public static int PrimaryBalanceMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 26249.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    public static int PrimaryBalanceMetricSpan(int start, int end)
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
}
