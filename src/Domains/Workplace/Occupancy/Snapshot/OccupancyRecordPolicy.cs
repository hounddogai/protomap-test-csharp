namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of occupancy record policy.</summary>
public static class OccupancyRecordPolicy
{
    public static string OccupancyRecordBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancyRecord()
    {
        var policy = Retry.DefaultPolicy("occupancy-record");
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

    /// <summary>Puts a occupancy record code in its canonical OR- form.</summary>
    public static string NormalizeOccupancyRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
