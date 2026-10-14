namespace Acme.Workforce.Compliance.Exception.RollingExceptionGroup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled exception draft policy.</summary>
public static class ScheduledExceptionDraftPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledExceptionDraft()
    {
        var policy = Retry.DefaultPolicy("scheduled-exception-draft");
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

    /// <summary>Puts a scheduled exception draft code in its canonical SED- form.</summary>
    public static string NormalizeScheduledExceptionDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("SED-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"SED-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double ScheduledExceptionDraftWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.17;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
