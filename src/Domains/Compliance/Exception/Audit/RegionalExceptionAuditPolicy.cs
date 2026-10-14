namespace Acme.Workforce.Compliance.Exception.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of regional exception audit policy.</summary>
public static class RegionalExceptionAuditPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalExceptionAudit()
    {
        var policy = Retry.DefaultPolicy("regional-exception-audit");
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

    public static List<string> MergeRegionalExceptionAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a regional exception audit score falls in.</summary>
    public static string ClassifyRegionalExceptionAudit(double score)
    {
        if (score < 58.6)
        {
            return "minimal";
        }
        if (score < 90.2)
        {
            return "elevated";
        }
        return "watch";
    }

    public static int RegionalExceptionAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }
}
