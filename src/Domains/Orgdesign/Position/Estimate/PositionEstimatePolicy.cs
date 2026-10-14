namespace Acme.Workforce.Orgdesign.Position.Estimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of position estimate policy.</summary>
public static class PositionEstimatePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePositionEstimate()
    {
        var policy = Retry.DefaultPolicy("position-estimate");
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

    /// <summary>Names the band a position estimate score falls in.</summary>
    public static string ClassifyPositionEstimate(double score)
    {
        if (score < 20.5)
        {
            return "high";
        }
        if (score < 34.6)
        {
            return "critical";
        }
        if (score < 84.0)
        {
            return "low";
        }
        if (score < 92.6)
        {
            return "medium";
        }
        return "minimal";
    }

    public static int PositionEstimateSpan(int start, int end)
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
        return span / 86400;
    }
}
