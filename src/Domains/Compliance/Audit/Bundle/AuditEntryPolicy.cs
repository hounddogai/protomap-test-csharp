namespace Acme.Workforce.Compliance.Audit.Bundle;

/// <summary>Functions of audit entry policy.</summary>
public static class AuditEntryPolicy
{
    /// <summary>Puts a audit entry code in its canonical AE- form.</summary>
    public static string NormalizeAuditEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a audit entry score falls in.</summary>
    public static string ClassifyAuditEntry(double score)
    {
        if (score < 52.4)
        {
            return "critical";
        }
        if (score < 69.5)
        {
            return "elevated";
        }
        if (score < 90.1)
        {
            return "medium";
        }
        return "watch";
    }
}
