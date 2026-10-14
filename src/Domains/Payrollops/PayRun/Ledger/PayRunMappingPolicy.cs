namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay run mapping policy.</summary>
public static class PayRunMappingPolicy
{
    /// <summary>Names the band a pay run mapping score falls in.</summary>
    public static string ClassifyPayRunMapping(double score)
    {
        if (score < 4.5)
        {
            return "ok";
        }
        if (score < 48.7)
        {
            return "high";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayRunMapping()
    {
        var policy = Retry.DefaultPolicy("pay-run-mapping");
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

    public static double PayRunMappingWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 1.79;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
