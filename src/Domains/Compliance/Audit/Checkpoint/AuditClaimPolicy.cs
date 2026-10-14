namespace Acme.Workforce.Compliance.Audit.Checkpoint;

/// <summary>Functions of audit claim policy.</summary>
public static class AuditClaimPolicy
{
    /// <summary>Puts a audit claim code in its canonical AC- form.</summary>
    public static string NormalizeAuditClaimCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("AC-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"AC-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Names the band a audit claim score falls in.</summary>
    public static string ClassifyAuditClaim(double score)
    {
        if (score < 22.3)
        {
            return "minimal";
        }
        if (score < 40.1)
        {
            return "low";
        }
        if (score < 78.1)
        {
            return "watch";
        }
        return "ok";
    }
}
