namespace Acme.Workforce.Compliance.Audit.Mapping;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of audit mapping policy.</summary>
public static class AuditMappingPolicy
{
    public static int AuditMappingSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Puts a audit mapping code in its canonical AM- form.</summary>
    public static string NormalizeAuditMappingCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAuditMapping()
    {
        var policy = Retry.DefaultPolicy("audit-mapping");
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
}
