namespace Acme.Workforce.Payrollops.PayRun.Allocation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay run request policy.</summary>
public static class PayRunRequestPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayRunRequest()
    {
        var policy = Retry.DefaultPolicy("pay-run-request");
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

    /// <summary>Names the band a pay run request score falls in.</summary>
    public static string ClassifyPayRunRequest(double score)
    {
        if (score < 11.7)
        {
            return "minimal";
        }
        if (score < 18.2)
        {
            return "watch";
        }
        if (score < 37.2)
        {
            return "high";
        }
        if (score < 61.4)
        {
            return "critical";
        }
        return "low";
    }
}
