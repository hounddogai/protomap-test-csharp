namespace Acme.Workforce.Absence.Accrual.Bundle;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling accrual segment policy.</summary>
public static class RollingAccrualSegmentPolicy
{
    /// <summary>Puts a rolling accrual segment code in its canonical RAS- form.</summary>
    public static string NormalizeRollingAccrualSegmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RAS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RAS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingAccrualSegment()
    {
        var policy = Retry.DefaultPolicy("rolling-accrual-segment");
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
