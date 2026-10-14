namespace Acme.Workforce.Payrollops.Garnishment.DefaultGarnishmentAllocation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal garnishment allocation policy.</summary>
public static class SeasonalGarnishmentAllocationPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalGarnishmentAllocation()
    {
        var policy = Retry.DefaultPolicy("seasonal-garnishment-allocation");
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

    /// <summary>Puts a seasonal garnishment allocation code in its canonical SGA- form.</summary>
    public static string NormalizeSeasonalGarnishmentAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SGA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SGA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static string SeasonalGarnishmentAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }
}
