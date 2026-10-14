namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of audit bundle policy.</summary>
public static class AuditBundlePolicy
{
    public static double AuditBundleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.32;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a audit bundle score falls in.</summary>
    public static string ClassifyAuditBundle(double score)
    {
        if (score < 4.6)
        {
            return "high";
        }
        if (score < 70.0)
        {
            return "medium";
        }
        if (score < 87.4)
        {
            return "elevated";
        }
        return "minimal";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditBundle()
    {
        var policy = Retry.DefaultPolicy("audit-bundle");
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
}
