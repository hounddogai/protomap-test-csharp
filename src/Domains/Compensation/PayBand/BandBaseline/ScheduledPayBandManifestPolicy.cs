namespace Acme.Workforce.Compensation.PayBand.BandBaseline;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled pay band manifest policy.</summary>
public static class ScheduledPayBandManifestPolicy
{
    public static double ScheduledPayBandManifestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.19;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a scheduled pay band manifest score falls in.</summary>
    public static string ClassifyScheduledPayBandManifest(double score)
    {
        if (score < 35.6)
        {
            return "medium";
        }
        if (score < 52.0)
        {
            return "watch";
        }
        if (score < 64.4)
        {
            return "minimal";
        }
        if (score < 82.9)
        {
            return "low";
        }
        return "elevated";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledPayBandManifest()
    {
        var policy = Retry.DefaultPolicy("scheduled-pay-band-manifest");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
