namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of default retro pay allocation policy.</summary>
public static class DefaultRetroPayAllocationPolicy
{
    public static string DefaultRetroPayAllocationBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 25.0, 50.0);
        return $"{lower}-{lower + 50.0}";
    }

    /// <summary>Names the band a default retro pay allocation score falls in.</summary>
    public static string ClassifyDefaultRetroPayAllocation(double score)
    {
        if (score < 29.5)
        {
            return "minimal";
        }
        if (score < 51.9)
        {
            return "elevated";
        }
        if (score < 62.7)
        {
            return "high";
        }
        return "low";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeDefaultRetroPayAllocation()
    {
        var policy = Retry.DefaultPolicy("default-retro-pay-allocation");
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

    /// <summary>Puts a default retro pay allocation code in its canonical DRPA- form.</summary>
    public static string NormalizeDefaultRetroPayAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("DRPA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"DRPA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
