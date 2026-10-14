namespace Acme.Workforce.Contractors.StatementOfWork.Target;

/// <summary>Functions of adaptive statement of work audit policy.</summary>
public static class AdaptiveStatementOfWorkAuditPolicy
{
    /// <summary>Names the band a adaptive statement of work audit score falls in.</summary>
    public static string ClassifyAdaptiveStatementOfWorkAudit(double score)
    {
        if (score < 22.3)
        {
            return "watch";
        }
        if (score < 35.4)
        {
            return "medium";
        }
        if (score < 62.8)
        {
            return "minimal";
        }
        if (score < 99.9)
        {
            return "low";
        }
        return "high";
    }

    /// <summary>Puts a adaptive statement of work audit code in its canonical ASOWA- form.</summary>
    public static string NormalizeAdaptiveStatementOfWorkAuditCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ASOWA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ASOWA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
