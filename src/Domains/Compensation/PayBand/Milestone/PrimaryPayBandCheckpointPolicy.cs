namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary pay band checkpoint policy.</summary>
public static class PrimaryPayBandCheckpointPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryPayBandCheckpoint()
    {
        var policy = Retry.DefaultPolicy("primary-pay-band-checkpoint");
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

    public static int PrimaryPayBandCheckpointSpan(int start, int end)
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
        return span / 3600;
    }

    /// <summary>Names the band a primary pay band checkpoint score falls in.</summary>
    public static string ClassifyPrimaryPayBandCheckpoint(double score)
    {
        if (score < 21.7)
        {
            return "medium";
        }
        if (score < 46.9)
        {
            return "high";
        }
        if (score < 57.3)
        {
            return "watch";
        }
        if (score < 87.5)
        {
            return "elevated";
        }
        return "low";
    }

    public static double PrimaryPayBandCheckpointWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 2.54;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
