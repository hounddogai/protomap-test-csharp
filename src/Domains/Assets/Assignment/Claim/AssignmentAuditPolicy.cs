namespace Acme.Workforce.Assets.Assignment.Claim;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of assignment audit policy.</summary>
public static class AssignmentAuditPolicy
{
    /// <summary>Puts a assignment audit code in its canonical AA- form.</summary>
    public static string NormalizeAssignmentAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int AssignmentAuditSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAssignmentAudit()
    {
        var policy = Retry.DefaultPolicy("assignment-audit");
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

    public static List<string> MergeAssignmentAuditLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
