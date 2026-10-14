namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of audit group policy.</summary>
public static class AuditGroupPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditGroup()
    {
        var policy = Retry.DefaultPolicy("audit-group");
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

    /// <summary>Puts a audit group code in its canonical AG- form.</summary>
    public static string NormalizeAuditGroupCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AG-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AG-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAuditGroupLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
