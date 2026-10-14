namespace Acme.Workforce.Workplace.Desk.Rule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of pending desk rollup policy.</summary>
public static class PendingDeskRollupPolicy
{
    /// <summary>Puts a pending desk rollup code in its canonical PDR- form.</summary>
    public static string NormalizePendingDeskRollupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PDR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PDR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingDeskRollup()
    {
        var policy = Retry.DefaultPolicy("pending-desk-rollup");
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

    public static double PendingDeskRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.89;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
