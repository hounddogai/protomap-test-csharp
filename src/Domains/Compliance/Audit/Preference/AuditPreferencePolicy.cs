namespace Acme.Workforce.Compliance.Audit.Preference;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of audit preference policy.</summary>
public static class AuditPreferencePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditPreference()
    {
        var policy = Retry.DefaultPolicy("audit-preference");
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

    /// <summary>Names the band a audit preference score falls in.</summary>
    public static string ClassifyAuditPreference(double score)
    {
        if (score < 27.0)
        {
            return "critical";
        }
        if (score < 69.9)
        {
            return "watch";
        }
        return "high";
    }

    public static int AuditPreferenceSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    public static double AuditPreferenceWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.77;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
