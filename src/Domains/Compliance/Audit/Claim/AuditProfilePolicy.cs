namespace Acme.Workforce.Compliance.Audit.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of audit profile policy.</summary>
public static class AuditProfilePolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditProfile()
    {
        var policy = Retry.DefaultPolicy("audit-profile");
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

    public static List<string> MergeAuditProfileLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a audit profile score falls in.</summary>
    public static string ClassifyAuditProfile(double score)
    {
        if (score < 19.1)
        {
            return "watch";
        }
        if (score < 27.1)
        {
            return "high";
        }
        if (score < 68.0)
        {
            return "elevated";
        }
        if (score < 75.2)
        {
            return "critical";
        }
        return "minimal";
    }
}
