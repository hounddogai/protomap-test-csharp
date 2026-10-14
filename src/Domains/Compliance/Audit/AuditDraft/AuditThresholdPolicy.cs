namespace Acme.Workforce.Compliance.Audit.AuditDraft;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of audit threshold policy.</summary>
public static class AuditThresholdPolicy
{
    public static string AuditThresholdBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditThreshold()
    {
        var policy = Retry.DefaultPolicy("audit-threshold");
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

    /// <summary>Puts a audit threshold code in its canonical AT- form.</summary>
    public static string NormalizeAuditThresholdCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static double AuditThresholdWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.9;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
