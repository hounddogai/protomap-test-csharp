namespace Acme.Workforce.Payrollops.PayRun.RunAudit;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal pay run batch policy.</summary>
public static class SeasonalPayRunBatchPolicy
{
    /// <summary>Puts a seasonal pay run batch code in its canonical SPRB- form.</summary>
    public static string NormalizeSeasonalPayRunBatchCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SPRB-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SPRB-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double SeasonalPayRunBatchWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.37;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalPayRunBatch()
    {
        var policy = Retry.DefaultPolicy("seasonal-pay-run-batch");
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
