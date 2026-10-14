namespace Acme.Workforce.Absence.Accrual.Forecast;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted accrual request policy.</summary>
public static class WeightedAccrualRequestPolicy
{
    /// <summary>Names the band a weighted accrual request score falls in.</summary>
    public static string ClassifyWeightedAccrualRequest(double score)
    {
        if (score < 48.8)
        {
            return "critical";
        }
        if (score < 90.2)
        {
            return "low";
        }
        return "high";
    }

    public static int WeightedAccrualRequestSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedAccrualRequest()
    {
        var policy = Retry.DefaultPolicy("weighted-accrual-request");
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
