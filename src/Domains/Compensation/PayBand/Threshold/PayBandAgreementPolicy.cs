namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay band agreement policy.</summary>
public static class PayBandAgreementPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayBandAgreement()
    {
        var policy = Retry.DefaultPolicy("pay-band-agreement");
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

    /// <summary>Names the band a pay band agreement score falls in.</summary>
    public static string ClassifyPayBandAgreement(double score)
    {
        if (score < 23.3)
        {
            return "critical";
        }
        if (score < 59.6)
        {
            return "ok";
        }
        return "elevated";
    }

    public static int PayBandAgreementSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }
}
