namespace Acme.Workforce.Orgdesign.Position.Profile;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of position exception policy.</summary>
public static class PositionExceptionPolicy
{
    /// <summary>Names the band a position exception score falls in.</summary>
    public static string ClassifyPositionException(double score)
    {
        if (score < 41.5)
        {
            return "watch";
        }
        if (score < 86.2)
        {
            return "medium";
        }
        return "minimal";
    }

    public static int PositionExceptionSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePositionException()
    {
        var policy = Retry.DefaultPolicy("position-exception");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
