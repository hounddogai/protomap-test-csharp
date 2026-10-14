namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default exception quota policy.</summary>
public static class DefaultExceptionQuotaPolicy
{
    public static string DefaultExceptionQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultExceptionQuota()
    {
        var policy = Retry.DefaultPolicy("default-exception-quota");
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

    /// <summary>Names the band a default exception quota score falls in.</summary>
    public static string ClassifyDefaultExceptionQuota(double score)
    {
        if (score < 14.8)
        {
            return "watch";
        }
        if (score < 50.0)
        {
            return "low";
        }
        if (score < 82.9)
        {
            return "medium";
        }
        return "critical";
    }

    /// <summary>Counts how many amounts fit in a budget of 31864.0, in order.</summary>
    public static int DefaultExceptionQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31864.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
