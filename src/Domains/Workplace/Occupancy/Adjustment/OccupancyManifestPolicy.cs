namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of occupancy manifest policy.</summary>
public static class OccupancyManifestPolicy
{
    /// <summary>Names the band a occupancy manifest score falls in.</summary>
    public static string ClassifyOccupancyManifest(double score)
    {
        if (score < 11.0)
        {
            return "low";
        }
        if (score < 49.4)
        {
            return "elevated";
        }
        if (score < 77.7)
        {
            return "ok";
        }
        return "critical";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancyManifest()
    {
        var policy = Retry.DefaultPolicy("occupancy-manifest");
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

    /// <summary>Puts a occupancy manifest code in its canonical OM- form.</summary>
    public static string NormalizeOccupancyManifestCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int OccupancyManifestSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }
}
