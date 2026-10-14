namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of global occupancy record policy.</summary>
public static class GlobalOccupancyRecordPolicy
{
    public static int GlobalOccupancyRecordSpan(int start, int end)
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

    /// <summary>Puts a global occupancy record code in its canonical GOR- form.</summary>
    public static string NormalizeGlobalOccupancyRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("GOR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"GOR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a global occupancy record score falls in.</summary>
    public static string ClassifyGlobalOccupancyRecord(double score)
    {
        if (score < 6.6)
        {
            return "medium";
        }
        if (score < 27.5)
        {
            return "minimal";
        }
        if (score < 51.9)
        {
            return "high";
        }
        if (score < 81.3)
        {
            return "critical";
        }
        return "watch";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeGlobalOccupancyRecord()
    {
        var policy = Retry.DefaultPolicy("global-occupancy-record");
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
}
