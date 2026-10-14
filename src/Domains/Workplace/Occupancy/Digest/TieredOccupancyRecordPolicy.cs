namespace Acme.Workforce.Workplace.Occupancy.Digest;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered occupancy record policy.</summary>
public static class TieredOccupancyRecordPolicy
{
    /// <summary>Puts a tiered occupancy record code in its canonical TOR- form.</summary>
    public static string NormalizeTieredOccupancyRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TOR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TOR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int TieredOccupancyRecordSpan(int start, int end)
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
    public static int ProbeTieredOccupancyRecord()
    {
        var policy = Retry.DefaultPolicy("tiered-occupancy-record");
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
