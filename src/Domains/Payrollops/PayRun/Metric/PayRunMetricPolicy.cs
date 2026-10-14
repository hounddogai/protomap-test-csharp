namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay run metric policy.</summary>
public static class PayRunMetricPolicy
{
    /// <summary>Names the band a pay run metric score falls in.</summary>
    public static string ClassifyPayRunMetric(double score)
    {
        if (score < 20.1)
        {
            return "low";
        }
        if (score < 28.2)
        {
            return "ok";
        }
        if (score < 32.6)
        {
            return "watch";
        }
        if (score < 90.8)
        {
            return "medium";
        }
        return "elevated";
    }

    public static string PayRunMetricBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayRunMetric()
    {
        var policy = Retry.DefaultPolicy("pay-run-metric");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
