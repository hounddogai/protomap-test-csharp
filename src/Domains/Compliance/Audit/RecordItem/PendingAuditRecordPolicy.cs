namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pending audit record policy.</summary>
public static class PendingAuditRecordPolicy
{
    public static List<string> MergePendingAuditRecordLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePendingAuditRecord()
    {
        var policy = Retry.DefaultPolicy("pending-audit-record");
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

    /// <summary>Puts a pending audit record code in its canonical PAR- form.</summary>
    public static string NormalizePendingAuditRecordCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("PAR-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"PAR-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
