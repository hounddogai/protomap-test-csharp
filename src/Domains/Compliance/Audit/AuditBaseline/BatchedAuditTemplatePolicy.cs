namespace Acme.Workforce.Compliance.Audit.AuditBaseline;

/// <summary>Functions of batched audit template policy.</summary>
public static class BatchedAuditTemplatePolicy
{
    public static int BatchedAuditTemplateSpan(int start, int end)
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
        return span / 60;
    }

    /// <summary>Puts a batched audit template code in its canonical BAT- form.</summary>
    public static string NormalizeBatchedAuditTemplateCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("BAT-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"BAT-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
