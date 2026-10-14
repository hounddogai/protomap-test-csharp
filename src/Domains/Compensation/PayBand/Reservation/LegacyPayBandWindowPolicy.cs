namespace Acme.Workforce.Compensation.PayBand.Reservation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of legacy pay band window policy.</summary>
public static class LegacyPayBandWindowPolicy
{
    public static string LegacyPayBandWindowBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeLegacyPayBandWindow()
    {
        var policy = Retry.DefaultPolicy("legacy-pay-band-window");
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

    /// <summary>Names the band a legacy pay band window score falls in.</summary>
    public static string ClassifyLegacyPayBandWindow(double score)
    {
        if (score < 12.9)
        {
            return "critical";
        }
        if (score < 23.0)
        {
            return "low";
        }
        if (score < 23.9)
        {
            return "medium";
        }
        if (score < 47.2)
        {
            return "elevated";
        }
        return "minimal";
    }
}
