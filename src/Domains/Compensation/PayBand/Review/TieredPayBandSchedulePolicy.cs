namespace Acme.Workforce.Compensation.PayBand.Review;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered pay band schedule policy.</summary>
public static class TieredPayBandSchedulePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredPayBandSchedule()
    {
        var policy = Retry.DefaultPolicy("tiered-pay-band-schedule");
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

    /// <summary>Names the band a tiered pay band schedule score falls in.</summary>
    public static string ClassifyTieredPayBandSchedule(double score)
    {
        if (score < 87.8)
        {
            return "low";
        }
        if (score < 90.4)
        {
            return "critical";
        }
        return "minimal";
    }
}
