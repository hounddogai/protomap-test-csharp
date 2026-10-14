namespace Acme.Workforce.Workplace.Occupancy.OccupancyPlan;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of occupancy exception policy.</summary>
public static class OccupancyExceptionPolicy
{
    /// <summary>Puts a occupancy exception code in its canonical OE- form.</summary>
    public static string NormalizeOccupancyExceptionCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string OccupancyExceptionBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancyException()
    {
        var policy = Retry.DefaultPolicy("occupancy-exception");
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
}
