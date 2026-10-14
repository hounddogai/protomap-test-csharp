namespace Acme.Workforce.Payrollops.PayRun.Allocation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pay run allocation policy.</summary>
public static class PayRunAllocationPolicy
{
    /// <summary>Names the band a pay run allocation score falls in.</summary>
    public static string ClassifyPayRunAllocation(double score)
    {
        if (score < 40.1)
        {
            return "high";
        }
        if (score < 73.4)
        {
            return "elevated";
        }
        return "critical";
    }

    /// <summary>Puts a pay run allocation code in its canonical PRA- form.</summary>
    public static string NormalizePayRunAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PRA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PRA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePayRunAllocation()
    {
        var policy = Retry.DefaultPolicy("pay-run-allocation");
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

    public static string PayRunAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }
}
