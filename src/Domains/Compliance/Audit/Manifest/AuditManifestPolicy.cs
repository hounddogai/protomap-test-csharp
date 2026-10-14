namespace Acme.Workforce.Compliance.Audit.Manifest;

/// <summary>Functions of audit manifest policy.</summary>
public static class AuditManifestPolicy
{
    public static int AuditManifestSpan(int start, int end)
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

    /// <summary>Puts a audit manifest code in its canonical AM- form.</summary>
    public static string NormalizeAuditManifestCode(string code)
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
}
