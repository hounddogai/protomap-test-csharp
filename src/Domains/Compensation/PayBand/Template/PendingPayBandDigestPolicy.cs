namespace Acme.Workforce.Compensation.PayBand.Template;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending pay band digest policy.</summary>
public static class PendingPayBandDigestPolicy
{
    /// <summary>Names the band a pending pay band digest score falls in.</summary>
    public static string ClassifyPendingPayBandDigest(double score)
    {
        if (score < 13.8)
        {
            return "medium";
        }
        if (score < 43.3)
        {
            return "ok";
        }
        if (score < 54.5)
        {
            return "low";
        }
        if (score < 84.6)
        {
            return "minimal";
        }
        return "high";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingPayBandDigest()
    {
        var policy = Retry.DefaultPolicy("pending-pay-band-digest");
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

    public static double PendingPayBandDigestWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 99) * 2.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
