namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of capped pay band quota policy.</summary>
public static class CappedPayBandQuotaPolicy
{
    public static string CappedPayBandQuotaBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Names the band a capped pay band quota score falls in.</summary>
    public static string ClassifyCappedPayBandQuota(double score)
    {
        if (score < 63.0)
        {
            return "low";
        }
        if (score < 65.7)
        {
            return "watch";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeCappedPayBandQuota()
    {
        var policy = Retry.DefaultPolicy("capped-pay-band-quota");
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
