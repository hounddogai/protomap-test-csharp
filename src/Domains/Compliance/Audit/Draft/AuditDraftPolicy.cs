namespace Acme.Workforce.Compliance.Audit.Draft;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of audit draft policy.</summary>
public static class AuditDraftPolicy
{
    public static int AuditDraftSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 60)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditDraft()
    {
        var policy = Retry.DefaultPolicy("audit-draft");
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

    /// <summary>Names the band a audit draft score falls in.</summary>
    public static string ClassifyAuditDraft(double score)
    {
        if (score < 17.4)
        {
            return "minimal";
        }
        if (score < 20.8)
        {
            return "watch";
        }
        if (score < 55.9)
        {
            return "ok";
        }
        return "low";
    }

    /// <summary>Puts a audit draft code in its canonical AD- form.</summary>
    public static string NormalizeAuditDraftCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AD-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AD-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
